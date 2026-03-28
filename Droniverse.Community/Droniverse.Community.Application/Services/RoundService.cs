using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class RoundService : IRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly IClock _clock;

        public RoundService(IUnitOfWork unitOfWork, AcademyMicroserviceClient academyMicroserviceClient, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
            _clock = clock;
        }

        public async Task<RoundResponseDto> CreateRound(RoundCreateDto request)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == request.CompetitionID);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {request.CompetitionID} not found.");

            await ValidateRoundData(request.CompetitionID, request.LabID, request.RoundNumber, request.StartTime, request.EndTime, competition, null);

            var round = new Round(
                request.CompetitionID,
                request.LabID,
                request.RoundNumber,
                request.StartTime,
                request.EndTime
            );

            await _unitOfWork.Rounds.Add(round);
            await _unitOfWork.SaveChangeAsync();

            return await MapToRoundResponse(round);
        }

        public async Task<RoundResponseDto> UpdateRound(Guid id, RoundUpdateDto request)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(
                r => r.RoundID == id,
                q => q.Include(r => r.UserRounds).Include(r => r.Competition)
            );

            if (round == null)
                throw new KeyNotFoundException($"Round with ID {id} not found.");

            var competition = round.Competition;
            if (competition == null)
            {
                competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == round.CompetitionID);
                if (competition == null)
                    throw new KeyNotFoundException($"Competition with ID {round.CompetitionID} not found.");
            }

            await ValidateRoundData(round.CompetitionID, request.LabID, request.RoundNumber, request.StartTime, request.EndTime, competition, id);

            round.UpdateInfo(
                request.LabID,
                request.RoundNumber,
                request.StartTime,
                request.EndTime
            );

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await MapToRoundResponse(round);
        }

        public async Task<RoundResponseDto> GetRoundById(Guid id)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(
                r => r.RoundID == id,
                q => q.Include(r => r.UserRounds)
            );

            if (round == null)
                throw new KeyNotFoundException($"Round with ID {id} not found.");

            return await MapToRoundResponse(round);
        }

        public async Task<IEnumerable<RoundResponseDto>> GetRoundsByCompetition(Guid competitionId)
        {
            var competition = await _unitOfWork.Competitions.GetByCondition(c => c.CompetitionID == competitionId);
            if (competition == null)
                throw new KeyNotFoundException($"Competition with ID {competitionId} not found.");

            var rounds = await _unitOfWork.Rounds.GetManyByCondition(
                r => r.CompetitionID == competitionId,
                q => q.Include(r => r.UserRounds).OrderBy(r => r.RoundNumber)
            );

            return rounds.Select(r => new RoundResponseDto
            {
                RoundID = r.RoundID,
                CompetitionID = r.CompetitionID,
                LabID = r.LabID,
                RoundNumber = r.RoundNumber,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                TotalParticipants = r.UserRounds?.Count ?? 0
            });
        }

        public async Task<RoundResponseDto> StartRound(Guid id)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(
                r => r.RoundID == id,
                q => q.Include(r => r.UserRounds)
            );

            if (round == null)
                throw new KeyNotFoundException($"Round with ID {id} not found.");

            round.StartRound(_clock.Now);

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await MapToRoundResponse(round);
        }

        public async Task<RoundResponseDto> FinishRound(Guid id)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(
                r => r.RoundID == id,
                q => q.Include(r => r.UserRounds)
            );

            if (round == null)
                throw new KeyNotFoundException($"Round with ID {id} not found.");

            round.FinishRound(_clock.Now);

            await _unitOfWork.Rounds.Update(round);
            await _unitOfWork.SaveChangeAsync();

            return await MapToRoundResponse(round);
        }

        public async Task<IEnumerable<RoundLeaderboardEntryDto>> GetRoundLeaderboard(Guid roundId)
        {
            var round = await _unitOfWork.Rounds.GetByCondition(r => r.RoundID == roundId);
            if (round == null)
                throw new KeyNotFoundException($"Round with ID {roundId} not found.");

            var userRounds = await _unitOfWork.UserRounds.GetManyByCondition(
                ur => ur.RoundID == roundId && ur.IsCompleted,
                q => q.OrderByDescending(ur => ur.Point)
                      .ThenBy(ur => ur.ExecutionTime)
                      .ThenBy(ur => ur.SubmittedAt)
            );

            return userRounds.Select((ur, index) => new RoundLeaderboardEntryDto
            {
                UserID = ur.UserID,
                Point = ur.Point,
                ExecutionTime = ur.ExecutionTime,
                NumberOfSteps = ur.NumberOfSteps,
                PathLength = ur.PathLength,
                IsCompleted = ur.IsCompleted,
                SubmittedAt = ur.SubmittedAt,
                Rank = index + 1
            });
        }

        private async Task ValidateRoundData(
            Guid competitionId,
            Guid labId,
            int roundNumber,
            DateTime startTime,
            DateTime endTime,
            Competition competition,
            Guid? excludeRoundId = null)
        {
            // Validate StartTime < EndTime
            if (startTime >= endTime)
                throw new InvalidOperationException("Round start time must be before end time.");

            // Validate thời gian Round nằm trong thời gian Competition
            if (startTime < competition.StartDate || startTime > competition.EndDate)
                throw new InvalidOperationException($"Round start time must be within competition period ({competition.StartDate:yyyy-MM-dd} to {competition.EndDate:yyyy-MM-dd}).");

            if (endTime < competition.StartDate || endTime > competition.EndDate)
                throw new InvalidOperationException($"Round end time must be within competition period ({competition.StartDate:yyyy-MM-dd} to {competition.EndDate:yyyy-MM-dd}).");

            // Lấy danh sách rounds của competition (exclude round hiện tại nếu đang update)
            var existingRounds = await _unitOfWork.Rounds.GetManyByCondition(
                r => r.CompetitionID == competitionId && (!excludeRoundId.HasValue || r.RoundID != excludeRoundId.Value)
            );

            // Validate RoundNumber không trùng
            var roundWithSameNumber = existingRounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
            if (roundWithSameNumber != null)
                throw new InvalidOperationException($"Round number {roundNumber} already exists for this competition.");

            // Validate thời gian không trùng với các round khác
            foreach (var existingRnd in existingRounds)
            {
                bool timeOverlap = (startTime >= existingRnd.StartTime && startTime < existingRnd.EndTime) ||
                                   (endTime > existingRnd.StartTime && endTime <= existingRnd.EndTime) ||
                                   (startTime <= existingRnd.StartTime && endTime >= existingRnd.EndTime);

                if (timeOverlap)
                    throw new InvalidOperationException($"Round time overlaps with Round {existingRnd.RoundNumber} ({existingRnd.StartTime:yyyy-MM-dd HH:mm} - {existingRnd.EndTime:yyyy-MM-dd HH:mm}).");
            }

            // Validate Lab không trùng với các round khác
            var roundWithSameLab = existingRounds.FirstOrDefault(r => r.LabID == labId);
            if (roundWithSameLab != null)
                throw new InvalidOperationException($"Lab này đã được chọn ở Round {roundWithSameLab.RoundNumber} rồi.");

            // Validate Lab tồn tại trong Academy Microservice (chỉ validate nếu là create hoặc LabID thay đổi)
            //if (!excludeRoundId.HasValue || (excludeRoundId.HasValue && existingRounds.All(r => r.LabID != labId)))
            //{
            //    var labExists = await _academyMicroserviceClient.IsLabExist(labId);
            //    if (!labExists)
            //        throw new KeyNotFoundException($"Lab with ID {labId} not found in Academy system.");
            //}
        }

        private async Task<RoundResponseDto> MapToRoundResponse(Round round)
        {
            var userRounds = await _unitOfWork.UserRounds.GetManyByCondition(ur => ur.RoundID == round.RoundID);

            return new RoundResponseDto
            {
                RoundID = round.RoundID,
                CompetitionID = round.CompetitionID,
                LabID = round.LabID,
                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                Status = round.Status,
                TotalParticipants = userRounds.Count()
            };
        }
    }
}
