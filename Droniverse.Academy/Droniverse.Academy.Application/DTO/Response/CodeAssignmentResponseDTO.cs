namespace Droniverse.Academy.Application.DTO.Response;

public class CodeAssignmentResponseDTO
{
    public string CodeId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class BulkCodeAssignmentResponseDTO  
{
    public int TotalAssigned { get; set; }
    public List<CodeAssignmentResponseDTO> AssignedItems { get; set; } = [];
}
