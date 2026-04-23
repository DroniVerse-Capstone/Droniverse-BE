using System;
using System.Threading.Tasks;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService
{
    public interface IUserSimulatorService
    {
        Task<SimulatorLearningStateDTO> GetSimulatorLearningStateAsync(Guid userLessonId);
        Task<bool> SubmitSimulatorAsync(Guid userLessonId, int flightTime, int? score);
    }
}
