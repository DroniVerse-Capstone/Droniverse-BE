using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class SubmitSolutionResponse
    {
        public Guid UserRoundId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public UserRoundStatus Status { get; set; } 
    }
}
