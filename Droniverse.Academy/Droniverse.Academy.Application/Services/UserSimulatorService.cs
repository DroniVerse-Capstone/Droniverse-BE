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

        public UserSimulatorService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<SimulatorLearningStateDTO> GetSimulatorLearningStateAsync(Guid userLessonId)
        {
            var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
                x => x.UserLessonID == userLessonId && x.UserID == _currentUser.UserId);

            if (userLesson == null)
                throw new BaseException("Không tìm thấy user lesson.", "NOT_FOUND");

            var lesson = await _unitOfWork.Lessons.GetByIdAsync(userLesson.LessonID)
                ?? throw new BaseException("Không tìm thấy lesson.", "NOT_FOUND");

            if (lesson.ReferenceID == Guid.Empty)
                throw new BaseException("Lesson simulator chưa được gán dữ liệu tham chiếu.", "NOT_FOUND");

            LessonClientViewDTO simulator = lesson.Type switch
            {
                LessonType.PHYSIC or LessonType.LAB_PHYSIC => await MapWebSimulatorAsync(lesson),
                LessonType.VR => await MapVrSimulatorAsync(lesson),
                _ => throw new ValidationException("Lesson không phải simulator."),
            };

            var userSimulator = await _unitOfWork.UserSimulators.GetByConditionAsync(
                x => x.UserLessonID == userLessonId);

            return new SimulatorLearningStateDTO
            {
                Simulator = simulator,
                UserSimulator = userSimulator == null ? null : _mapper.Map<UserSimulatorResponseDTO>(userSimulator)
            };
        }

        public async Task<bool> SubmitSimulatorAsync(Guid userLessonId, int flightTime, int? score)
        {
            var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
                x => x.UserLessonID == userLessonId && x.UserID == _currentUser.UserId);
            if (userLesson == null) return false;

            var entity = new UserSimulator
            {
                UserSimulatorID = Guid.NewGuid(),
                UserLessonID = userLessonId,
                FlightTime = flightTime,
                Score = score,
                IsSuccess = score.HasValue && score.Value > 0,
                UserLesson = userLesson
            };

            await _unitOfWork.UserSimulators.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<LessonClientViewDTO> MapWebSimulatorAsync(Lesson lesson)
        {
            var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(lesson.ReferenceID)
                ?? throw new BaseException("Không tìm thấy web simulator.", "NOT_FOUND");

            var simulator = _mapper.Map<LessonClientViewDTO>(webSimulator);
            simulator.LessonID = lesson.LessonID;
            simulator.ModuleID = lesson.ModuleID;
            simulator.OrderIndex = lesson.OrderIndex;
            simulator.Type = lesson.Type;
            simulator.ReferenceID = lesson.ReferenceID;
            return simulator;
        }

        private async Task<LessonClientViewDTO> MapVrSimulatorAsync(Lesson lesson)
        {
            var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(lesson.ReferenceID)
                ?? throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

            var simulator = _mapper.Map<LessonClientViewDTO>(vrSimulator);
            simulator.LessonID = lesson.LessonID;
            simulator.ModuleID = lesson.ModuleID;
            simulator.OrderIndex = lesson.OrderIndex;
            simulator.Type = lesson.Type;
            simulator.ReferenceID = lesson.ReferenceID;
            return simulator;
        }
    }
}
