using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public record ClubResponseDto
{
    public Guid ClubID { get; init; }
    public string NameVN { get; init; }
    public string NameEN { get; init; }
    public string DescriptionVN { get; init; }
    public string DescriptionEN { get; init; }
    public string ClubCode { get; init; }
    public ClubStatus Status { get; init; }
    public bool IsPublic { get; init; }
    public int LimitParticipation { get; init; }
    public int LimitClubManagers { get; init; }
    public int TotalMembers { get; set; }
    public int TotalCourses { get; set; }
    public UserResponse Creator { get; init; }
    public IEnumerable<CategoryResponseDto> Categories { get; init; }
}