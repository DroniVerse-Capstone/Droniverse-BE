using System.Text.Json.Serialization;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public record ClubPolicyResponseDto
{
    public Guid ClubPolicyID { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public ClubResponseDto Club { get; set; }
}
