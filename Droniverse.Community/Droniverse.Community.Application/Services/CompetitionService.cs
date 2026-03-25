using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
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

        public CompetitionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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
                request.RegistrationStartDate,
                request.RegistrationEndDate,
                request.StartDate,
                request.EndDate,
                currentUserId,
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

            // Lưu thời gian cũ để so sánh
            var oldStartDate = competition.StartDate;
            var oldEndDate = competition.EndDate;

            // Update thông tin competition
            competition.UpdateInformation(
                request.NameVN,
                request.NameEN,
                request.RuleContent,
                request.RegistrationStartDate,
                request.RegistrationEndDate,
                request.StartDate,
                request.EndDate,
                currentUserId,
                request.MaxParticipants,
                request.DescriptionVN,
                request.DescriptionEN
            );

            // Nếu thời gian competition thay đổi, validate lại các rounds
            if (oldStartDate != request.StartDate || oldEndDate != request.EndDate)
            {
                var invalidRoundIds = competition.ValidateAndMarkInvalidRounds();

                if (invalidRoundIds.Any())
                {
                    // Update các rounds bị invalid
                    foreach (var roundId in invalidRoundIds)
                    {
                        var round = competition.Rounds.FirstOrDefault(r => r.RoundID == roundId);
                        if (round != null)
                        {
                            await _unitOfWork.Rounds.Update(round);
                        }
                    }
                }
            }

            await _unitOfWork.Competitions.Update(competition);
            await _unitOfWork.SaveChangeAsync();

            return await MapToCompetitionResponse(competition);
        }

        public async Task<bool> DeleteCompetition(Guid id)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == id);
            if (competition == null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc thi với ID {id}.");

            // Chỉ cho phép xóa competition ở trạng thái DRAFT
            if (competition.Status != CompetitionStatus.DRAFT)
                throw new InvalidOperationException($"Không thể xóa cuộc thi khi đang ở trạng thái {competition.Status}. Chỉ có thể xóa cuộc thi ở trạng thái DRAFT.");

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
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            if (competition.Status != CompetitionStatus.OPEN)
                throw new InvalidOperationException("Cuộc thi không trong thời gian đăng ký.");

            var now = DateTime.UtcNow;
            if (now < competition.RegistrationStartDate || now > competition.RegistrationEndDate)
                throw new InvalidOperationException("Ngoài thời gian đăng ký cuộc thi.");

            var existingRegistration = await _unitOfWork.UserCompetitions.GetByCondition(
                uc => uc.UserID == currentUserId && uc.CompetitionID == competitionId
            );

            if (existingRegistration != null)
                throw new InvalidOperationException("Bạn đã đăng ký cuộc thi này rồi.");

            if (competition.MaxParticipants.HasValue)
            {
                var currentCount = competition.UserCompetitions.Count;
                if (currentCount >= competition.MaxParticipants.Value)
                    throw new InvalidOperationException("Cuộc thi đã đủ số lượng người tham gia.");
            }

            // Thiếu kiểm tra xem người dùng đã có certificate thõa mãn chưa bằng cách gọi tới Acadamy Service. Nếu không thõa mãn thì báo lỗi

            var userCompetition = new UserCompetition(currentUserId, competitionId);

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

            competition.FinishCompetition(currentUserId);

            await _unitOfWork.Competitions.Update(competition);
            await _unitOfWork.SaveChangeAsync();

            return await MapToCompetitionResponse(competition);
        }

        public async Task UpdateCompetitionStatusesAsync()
        {
            var now = new ClockService().Now;

            // Lấy các cuộc thi đang chưa hoàn thành hoặc chưa bị hủy
            var activeCompetitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => c.Status != CompetitionStatus.FINISHED && c.Status != CompetitionStatus.CANCELLED,
                q => q.Include(c => c.Rounds)
            );

            bool isModified = false;

            foreach (var competition in activeCompetitions)
            {
                bool changed = false;

                if (competition.CanAutoOpenRegistration(now))
                {
                    competition.SystemOpenRegistration();
                    changed = true;
                }
                else if (competition.CanAutoCloseRegistration(now))
                {
                    competition.SystemCloseRegistration();
                    changed = true;
                }
                else if (competition.CanAutoStartCompetition(now))
                {
                    try
                    {
                        competition.SystemStartCompetition();
                        changed = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        // gửi mail thông báo
                    }
                }
                else if (competition.CanAutoFinishCompetition(now))
                {
                    competition.SystemFinishCompetition();
                    changed = true;
                }
                else if ()
                {
                    // thời gian này không còn hợp lệ nữa
                    competition.SystemInvalidCompetition();
                    changed = true;
                }

                if (changed)
                {
                    await _unitOfWork.Competitions.Update(competition);
                    isModified = true;
                }
            }

            if (isModified)
            {
                await _unitOfWork.SaveChangeAsync();
            }
        }

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
            if (!competitionList.Any())
                return Enumerable.Empty<CompetitionResponse>();

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
