namespace Droniverse.Academy.Application.DTO.Response;

public class UserSimulatorResponseDTO
{
    public Guid UserSimulatorID { get; set; }
    public Guid UserLessonID { get; set; }
    public int FlightTime { get; set; }
    public int? Score { get; set; }
    public bool IsSuccess { get; set; }
}