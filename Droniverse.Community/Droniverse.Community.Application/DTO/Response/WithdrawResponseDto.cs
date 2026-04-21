using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response;

public record WithdrawResponseDto
{
    public string Username { get; set; }
    public decimal Amount { get; set; }


}