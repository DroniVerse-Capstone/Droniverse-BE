using System;
using System.Threading.Tasks;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService
{
    public interface IUserSimulatorService
    {
        Task<SimulatorLearningStateDTO> GetSimulatorLearningStateAsync(Guid enrollmentId, Guid lessonId);
        Task<bool> SubmitSimulatorAsync(Guid enrollmentId, Guid lessonId, int flightTime, int? score);
    }
}
