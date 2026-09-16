using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request;

/// <summary>
/// DTO cho vi?c search/filter ClubAttemptRequest
/// </summary>
public class ClubAttemptRequestSearchRequest : SearchRequest
{

    public ClubAttemptRequestStatus? Status { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? ProcessedFrom { get; set; }
    public DateTime? ProcessedTo { get; set; }
    public ClubAttemptRequestSortBy SortBy { get; set; } = ClubAttemptRequestSortBy.CreatedAt;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}

