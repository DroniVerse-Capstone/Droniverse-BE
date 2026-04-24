using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Application.DTO.Response;
using AutoMapper;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using System;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Services
{
    public class UserSimulatorService : IUserSimulatorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly ILearningService _learningService;

        public UserSimulatorService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper, ILearningService learningService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
            _learningService = learningService;
        }

        public async Task<SimulatorLearningStateDTO> GetSimulatorLearningStateAsync(Guid enrollmentId, Guid lessonId)
        {
            await _learningService.ValidateLessonAccessAsync(enrollmentId, lessonId);

            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId)
                ?? throw new BaseException("Không tìm thấy lesson.", "NOT_FOUND");

            if (lesson.ReferenceID == Guid.Empty)
                throw new BaseException("Lesson simulator chưa được gán dữ liệu tham chiếu.", "NOT_FOUND");

            var state = new SimulatorLearningStateDTO();

            switch (lesson.Type)
            {
                case LessonType.PHYSIC:
                case LessonType.LAB_PHYSIC:
                    state.WebSimulator = await MapWebSimulatorAsync(lesson);
                    break;
                case LessonType.VR:
                    state.VRSimulator = await MapVrSimulatorAsync(lesson);
                    break;
                default:
                    throw new ValidationException("Lesson không phải simulator.");
            }

            var userSimulator = await _unitOfWork.UserSimulators.GetByConditionAsync(
                x => x.UserID == _currentUser.UserId && x.LessonID == lessonId);

            state.UserSimulator = userSimulator == null ? null : _mapper.Map<UserSimulatorResponseDTO>(userSimulator);
            return state;
        }

        public async Task<bool> SubmitSimulatorAsync(Guid enrollmentId, Guid lessonId, int flightTime, int? score)
        {
            var lesson = await _unitOfWork.Lessons.GetByConditionAsync(
                x => x.LessonID == lessonId);
            if (lesson == null) return false;

            var entity = new UserSimulator
            {
                UserSimulatorID = Guid.NewGuid(),
                UserID = _currentUser.UserId,
                LessonID = lessonId,
                FlightTime = flightTime,
                Score = score,
                IsSuccess = score.HasValue && score.Value > 0
            };

            await _unitOfWork.UserSimulators.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _learningService.CompleteLessonBySimulatorSubmitAsync(enrollmentId, lessonId);
            return true;
        }

        private async Task<WebSimulatorClientViewDTO> MapWebSimulatorAsync(Lesson lesson)
        {
            var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(lesson.ReferenceID)
                ?? throw new BaseException("Không tìm thấy web simulator.", "NOT_FOUND");

            return new WebSimulatorClientViewDTO
            {
                WebSimulatorID = webSimulator.WebSimulatorID,
                DroneID = webSimulator.DroneID,
                TitleVN = webSimulator.TitleVN,
                TitleEN = webSimulator.TitleEN,
                Type = webSimulator.Type,
                ObjectivesVN = webSimulator.ObjectivesVN,
                ObjectivesEN = webSimulator.ObjectivesEN,
                Code = webSimulator.Code,
                EstimatedTime = webSimulator.EstimatedTime,
                CreateAt = webSimulator.CreateAt,
                UpdateAt = webSimulator.UpdateAt
            };
        }

        private async Task<VRSimulatorClientViewDTO> MapVrSimulatorAsync(Lesson lesson)
        {
            var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(lesson.ReferenceID)
                ?? throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

            return new VRSimulatorClientViewDTO
            {
                VRSimulatorID = vrSimulator.VRSimulatorID,
                TitleVN = vrSimulator.TitleVN,
                TitleEN = vrSimulator.TitleEN,
                EstimatedTime = vrSimulator.EstimatedTime,
                CreateAt = vrSimulator.CreateAt,
                UpdateAt = vrSimulator.UpdateAt
            };
        }
    }
}
