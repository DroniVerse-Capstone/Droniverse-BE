namespace Droniverse.Academy.Application.DTO.Response;

public class SimulatorLearningStateDTO
{
    public LessonClientViewDTO Simulator { get; set; } = null!;
    public UserSimulatorResponseDTO? UserSimulator { get; set; }
}