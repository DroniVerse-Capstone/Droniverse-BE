namespace Droniverse.Academy.Application.DTO.Response;

public class SimulatorLearningStateDTO
{
    public UserSimulatorResponseDTO? UserSimulator { get; set; }
    public VRSimulatorClientViewDTO? VRSimulator { get; set; }
    public WebSimulatorClientViewDTO? WebSimulator { get; set; }
}