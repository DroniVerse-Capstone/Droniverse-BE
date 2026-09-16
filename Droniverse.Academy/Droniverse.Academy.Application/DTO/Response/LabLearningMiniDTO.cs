namespace Droniverse.Academy.Application.DTO.Response;

public class LabLearningMiniDTO
{
    public LabClientViewDTO Lab { get; set; } = null!;
    public UserLabResponseDTO? UserLab { get; set; }
}
