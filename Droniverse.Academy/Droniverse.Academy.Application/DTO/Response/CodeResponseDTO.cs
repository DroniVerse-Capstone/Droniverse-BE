using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.DTO.Response;

public class CodeResponseDTO
{
    public string CodeID { get; set; }
    public CourseMiniResponse? Course { get; set; }
    public SimpleClubResponse? Club { get; set; }
    public UserResponse? OwnerUser { get; set; }
    public UserResponse? UsedByUser { get; set; }
    public DateTime? UsedDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
}

public class CodeUsageResponseDTO
{
    public string CodeID { get; set; }
    public Guid UserID { get; set; }
    public DateTime UsedDate { get; set; }
}
