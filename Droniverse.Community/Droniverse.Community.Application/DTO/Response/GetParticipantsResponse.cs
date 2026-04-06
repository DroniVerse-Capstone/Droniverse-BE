using Droniverse.Identity.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record GetParticipantsResponse
    {
        public Guid UserId { get; init; }
        public required string Username { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? ImageUrl { get; init; }
        public GenderOptions Gender { get; init; }
        public DateTime JoinDate { get; init; }
    }
}
