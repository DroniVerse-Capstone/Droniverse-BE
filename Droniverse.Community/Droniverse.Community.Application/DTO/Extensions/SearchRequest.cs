using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Extensions
{
    public class ParticipationSearchRequest : SearchRequest
    {
        public string? ParicipationName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
