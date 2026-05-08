using Droniverse.Identity.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.Application.DTO.Request;

public class NotificationSearchRequest : SearchRequest
{
    [FromQuery(Name = "status")]
    public NotificationStatus? Status { get; set; }

    [FromQuery(Name = "sentAt")]
    public DateTime? SentAt { get; set; }
}