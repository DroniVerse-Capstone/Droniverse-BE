namespace Droniverse.Academy.Application.DTO.Response;

public class UserSimulatorResponseDTO
{
    public Guid UserSimulatorID { get; set; }
    public Guid UserID { get; set; }
    public Guid LessonID { get; set; }
    public DateTime SubmitAt { get; set; }
    public int FlightTime { get; set; }
    public int? Score { get; set; }
    public bool IsSuccess { get; set; }
}