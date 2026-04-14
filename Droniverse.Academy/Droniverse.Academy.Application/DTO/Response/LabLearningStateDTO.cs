namespace Droniverse.Academy.Application.DTO.Response;

public class LabLearningStateDTO
{
    public LabClientViewDTO Lab { get; set; } = null!;
    public UserLabResponseDTO? UserLab { get; set; }
}
