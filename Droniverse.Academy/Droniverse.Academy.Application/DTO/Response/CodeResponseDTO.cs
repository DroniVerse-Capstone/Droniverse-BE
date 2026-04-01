using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class CodeResponseDTO
{
    public string CodeID { get; set; }
    public string CourseID { get; set; }
    public ICollection<CodeUsageResponseDTO> CodeUsages { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
}

public class CodeUsageResponseDTO
{
    public Guid CodeID { get; set; }
    public Guid UserID { get; set; }
    public DateTime UsedDate { get; set; }
}
