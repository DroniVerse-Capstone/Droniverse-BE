using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.Helpers;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;

        public CompetitionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _clock = clock;
        }

        public async Task<CompetitionResponse> CreateCompetition(CompetitionCreationRequest request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == request.ClubID);
            if (club == null)
                throw new KeyNotFoundException($"Không tìm thấy club với ID {request.ClubID}.");

            var competition = new Competition(
                request.ClubID,
                request.NameVN,
                request.NameEN,
                request.RuleContent,
                request.VisibleAt.TrimToMinute(),
                request.RegistrationStartDate.TrimToMinute(),
                request.RegistrationEndDate.TrimToMinute(),
                request.StartDate.TrimToMinute(),
                request.EndDate.TrimToMinute(),
                currentUserId,
                _clock.Now,
                request.MaxParticipants,
                request.DescriptionVN,
                request.DescriptionEN
                );

            await _unitOfWork.Competitions.Add(competition);
            await _unitOfWork.SaveChangeAsync();

            return await MapToCompetitionResponse(competition);
        }

        public async Task<CompetitionResponse> UpdateCompetition(Guid id, CompetitionUpdateDto request)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == id,
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID {id}.");

            // ===== 1. Lưu state cũ =====
            var oldStartDate = competition.StartDate;
            var oldEndDate = competition.EndDate;

            switch (competition.Status)
            {
                case CompetitionStatus.DRAFT:
                    competition.UpdateDraftInformation(
                        request.NameVN,
                        request.NameEN,
                        request.RuleContent,
                        request.VisibleAt, 
                        request.RegistrationStartDate,
                        request.RegistrationEndDate,
                        request.StartDate,
                        request.EndDate,
                        currentUserId,
                        _clock.Now,
                        request.MaxParticipants,
                        request.DescriptionVN,
                        request.DescriptionEN
                    );
                    break;

                case CompetitionStatus.PUBLISHED:
                    competition.UpdatePublishedInformation(
                        request.NameVN,
                        request.NameEN,
                        request.DescriptionVN,
                        request.DescriptionEN,
                        request.MaxParticipants,
                        request.RuleContent,
                        currentUserId,
                        _clock.Now
                    );
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Không thể cập nhật cuộc thi ở trạng thái [{competition.Status}].");
            }

            bool timelineChanged = oldStartDate != competition.StartDate || oldEndDate != competition.EndDate;

            if (timelineChanged)
                competition.ValidateAndMarkInvalidRounds();

            await _unitOfWork.SaveChangeAsync();

            return await MapToCompetitionResponse(competition);
        }

        public async Task<bool> DeleteCompetition(Guid id)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == id);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{id}].");

            if (competition.Status != CompetitionStatus.DRAFT)
                throw new InvalidOperationException($"Không thể xóa cuộc thi khi đang ở trạng thái [{competition.Status}]");

            await _unitOfWork.Competitions.Delete(competition);
            await _unitOfWork.SaveChangeAsync();

            return true;
        }

        public async Task<CompetitionResponse> GetCompetitionById(Guid id)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == id,
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {id} not found.");

            return await MapToCompetitionResponse(competition);
        }

        public async Task<IEnumerable<CompetitionResponse>> GetAllCompetitionsWithCondition(CompetitionSearchRequest searchRequest)
        {
            var competitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => (!searchRequest.Status.HasValue || c.Status == searchRequest.Status) &&
                     (string.IsNullOrEmpty(searchRequest.CompetitionName) ||
                      c.NameVN.Contains(searchRequest.CompetitionName) ||
                      c.NameEN.Contains(searchRequest.CompetitionName)),
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
                      .OrderByDescending(c => c.CreatedAt)
            );

            return await MapToCompetitionResponses(competitions);
        }

        public async Task<IEnumerable<CompetitionResponse>> GetCompetitionsByClub(Guid clubId, CompetitionStatus? status = null)
        {
            var competitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => c.ClubID == clubId && (!status.HasValue || c.Status == status.Value),
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            return await MapToCompetitionResponses(competitions);
        }

        public async Task<UserCompetitionResponseDto> RegisterForCompetition(Guid competitionId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.UserCompetitions)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID [{competitionId}].");

            // check user certificate
            //var isValid = await _academyService.CheckUserCertificate();

            //if (!isValid)
            //    throw new InvalidOperationException("Bạn chưa đủ điều kiện tham gia.");

            var now = new ClockService().Now;

            var userCompetition = competition.RegisterParticipant(
                currentUserId,
                now
            );

            await _unitOfWork.UserCompetitions.Add(userCompetition);
            await _unitOfWork.SaveChangeAsync();

            return new UserCompetitionResponseDto
            {
                UserCompetitionID = userCompetition.UserCompetitionID,
                UserID = userCompetition.UserID,
                CompetitionID = userCompetition.CompetitionID,
                Status = userCompetition.Status,
                Score = userCompetition.Score,
                Rank = userCompetition.Rank,
                PrizeID = userCompetition.PrizeID,
                CreatedAt = userCompetition.CreatedAt,
                UpdatedAt = userCompetition.UpdatedAt
            };
        }

        public async Task<UserCompetitionResponseDto> WithdrawFromCompetition(Guid competitionId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var userCompetition = await _unitOfWork.UserCompetitions.GetByCondition(
                uc => uc.UserID == currentUserId && uc.CompetitionID == competitionId
            );

            if (userCompetition == null)
                throw new KeyNotFoundException("Bạn chưa đăng ký cuộc thi này.");

            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            if (competition.Status == CompetitionStatus.ONGOING || competition.Status == CompetitionStatus.FINISHED)
                throw new InvalidOperationException("Không thể rút khỏi cuộc thi đã bắt đầu hoặc kết thúc.");

            userCompetition.Withdraw();

            await _unitOfWork.UserCompetitions.Update(userCompetition);
            await _unitOfWork.SaveChangeAsync();

            return new UserCompetitionResponseDto
            {
                UserCompetitionID = userCompetition.UserCompetitionID,
                UserID = userCompetition.UserID,
                CompetitionID = userCompetition.CompetitionID,
                Status = userCompetition.Status,
                Score = userCompetition.Score,
                Rank = userCompetition.Rank,
                PrizeID = userCompetition.PrizeID,
                CreatedAt = userCompetition.CreatedAt,
                UpdatedAt = userCompetition.UpdatedAt
            };
        }

        public async Task<IEnumerable<UserCompetitionResponseDto>> GetCompetitionParticipants(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            var participants = await _unitOfWork.UserCompetitions.GetManyByCondition(
                uc => uc.CompetitionID == competitionId && uc.Status == UserCompetitionStatus.ACTIVE
            );

            return participants.Select(uc => new UserCompetitionResponseDto
            {
                UserCompetitionID = uc.UserCompetitionID,
                UserID = uc.UserID,
                CompetitionID = uc.CompetitionID,
                Status = uc.Status,
                Score = uc.Score,
                Rank = uc.Rank,
                PrizeID = uc.PrizeID,
                CreatedAt = uc.CreatedAt,
                UpdatedAt = uc.UpdatedAt
            });
        }

        public async Task<IEnumerable<LeaderboardEntryDto>> GetCompetitionLeaderboard(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            var participants = await _unitOfWork.UserCompetitions.GetManyByCondition(
                uc => uc.CompetitionID == competitionId && uc.Status == UserCompetitionStatus.ACTIVE,
                q => q.OrderByDescending(uc => uc.Score).ThenBy(uc => uc.UpdatedAt)
            );

            return participants.Select((uc, index) => new LeaderboardEntryDto
            {
                UserID = uc.UserID,
                Score = uc.Score,
                Rank = uc.Rank ?? (index + 1),
                Status = uc.Status
            });
        }

        public async Task<CompetitionResponse> FinishCompetition(Guid competitionId)
        {
            var currentUserId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("User is not authenticated."));

            var competition = await _unitOfWork.Competitions.GetByCondition(
                c => c.CompetitionID == competitionId,
                q => q.Include(c => c.Rounds)
                      .Include(c => c.UserCompetitions)
                      .Include(c => c.CompetitionPrizes)
            );

            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            competition.FinishCompetition(currentUserId, _clock.Now);

            await _unitOfWork.Competitions.Update(competition);
            await _unitOfWork.SaveChangeAsync();

            return await MapToCompetitionResponse(competition);
        }

        //public async Task UpdateCompetitionStatusesAsync()
        //{
        //    var now = _clock.Now;

        //    var competitions = await _unitOfWork.Competitions.GetManyByCondition(
        //        c => c.Status != CompetitionStatus.FINISHED &&
        //             c.Status != CompetitionStatus.CANCELLED &&
        //             c.Status != CompetitionStatus.RESULT_PUBLISHED,
        //        q => q.Include(c => c.Rounds)
        //    );

        //    bool isModified = false;

        //    foreach (var competition in competitions)
        //    {
        //        bool changed = false;

        //        if (competition.CanAutoPublish(now))
        //        {
        //            competition.SystemPublish();
        //            changed = true;
        //        }

        //        if (competition.CanAutoOpenRegistration(now))
        //        {
        //            competition.SystemOpenRegistration();
        //            changed = true;
        //        }

        //        if (competition.CanAutoCloseRegistration(now))
        //        {
        //            competition.SystemCloseRegistration();
        //            changed = true;
        //        }

        //        if (competition.CanAutoInvalidCompetition(now))
        //        {
        //            competition.SystemInvalidCompetition();
        //            changed = true;
        //        }

        //        if (competition.CanAutoStartCompetition(now))
        //        {
        //            try
        //            {
        //                competition.SystemStartCompetition();
        //                changed = true;
        //            }
        //            catch (Exception ex)
        //            {
        //                // log + mark invalid nếu cần
        //                // _logger.LogError(ex, ...);
        //            }
        //        }

        //        if (competition.CanAutoFinishCompetition(now))
        //        {
        //            competition.SystemFinishCompetition();
        //            changed = true;
        //        }

        //        if (changed)
        //        {
        //            isModified = true;
        //        }
        //    }

        //    if (isModified)
        //    {
        //        await _unitOfWork.SaveChangeAsync();
        //    }
        //}

        private async Task<CompetitionResponse> MapToCompetitionResponse(Competition competition)
        {
            var competitionIds = new[] { competition.CompetitionID };
            var roundCounts = await _unitOfWork.Rounds.GetRoundCountsByCompetitionIds(competitionIds);
            var competitorCounts = await _unitOfWork.UserCompetitions.GetCompetitorCountsByCompetitionIds(competitionIds);
            var prizeCounts = await _unitOfWork.Competitions.GetPrizeCountsByCompetitionIds(competitionIds);

            return new CompetitionResponse
            {
                CompetitionID = competition.CompetitionID,
                ClubID = competition.ClubID,
                NameVN = competition.NameVN,
                NameEN = competition.NameEN,
                DescriptionVN = competition.DescriptionVN,
                DescriptionEN = competition.DescriptionEN,
                RuleContent = competition.RuleContent,
                MaxParticipants = competition.MaxParticipants,
                RegistrationStartDate = competition.RegistrationStartDate,
                RegistrationEndDate = competition.RegistrationEndDate,
                StartDate = competition.StartDate,
                EndDate = competition.EndDate,
                Status = competition.Status,
                ResultPublishedAt = competition.ResultPublishedAt,
                CreatedBy = competition.CreatedBy,
                UpdatedBy = competition.UpdatedBy,
                CreatedAt = competition.CreatedAt,
                UpdatedAt = competition.UpdatedAt,
                totalRounds = roundCounts.GetValueOrDefault(competition.CompetitionID, 0),
                totalCompetitors = competitorCounts.GetValueOrDefault(competition.CompetitionID, 0),
                totalPrizes = prizeCounts.GetValueOrDefault(competition.CompetitionID, 0)
            };
        }

        private async Task<IEnumerable<CompetitionResponse>> MapToCompetitionResponses(IEnumerable<Competition> competitions)
        {
            var competitionList = competitions.ToList();
            if (competitionList.Count == 0)
                return [];

            var competitionIds = competitionList.Select(c => c.CompetitionID).ToList();
            var roundCounts = await _unitOfWork.Rounds.GetRoundCountsByCompetitionIds(competitionIds);
            var competitorCounts = await _unitOfWork.UserCompetitions.GetCompetitorCountsByCompetitionIds(competitionIds);
            var prizeCounts = await _unitOfWork.Competitions.GetPrizeCountsByCompetitionIds(competitionIds);

            return competitionList.Select(competition => new CompetitionResponse
            {
                CompetitionID = competition.CompetitionID,
                ClubID = competition.ClubID,
                NameVN = competition.NameVN,
                NameEN = competition.NameEN,
                DescriptionVN = competition.DescriptionVN,
                DescriptionEN = competition.DescriptionEN,
                RuleContent = competition.RuleContent,
                MaxParticipants = competition.MaxParticipants,
                RegistrationStartDate = competition.RegistrationStartDate,
                RegistrationEndDate = competition.RegistrationEndDate,
                StartDate = competition.StartDate,
                EndDate = competition.EndDate,
                Status = competition.Status,
                ResultPublishedAt = competition.ResultPublishedAt,
                CreatedBy = competition.CreatedBy,
                UpdatedBy = competition.UpdatedBy,
                CreatedAt = competition.CreatedAt,
                UpdatedAt = competition.UpdatedAt,
                totalRounds = roundCounts.GetValueOrDefault(competition.CompetitionID, 0),
                totalCompetitors = competitorCounts.GetValueOrDefault(competition.CompetitionID, 0),
                totalPrizes = prizeCounts.GetValueOrDefault(competition.CompetitionID, 0)
            });
        }
    }
}
