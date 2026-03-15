using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
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

    public class CompetitionSearchRequest : SearchRequest
    {
        public string? CompetitionName { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [FromQuery]
        public CompetitionStatus? Status { get; set; }
    }

    public class ClubCreationRequestSearchRequest : SearchRequest
    {
        public ClubCreationRequestStatus? status { get; set; } = null;
    }
    public class ClubCourseSearchRequest : SearchRequest
    {
        // số người học
        // số người học
        // các course mà club sở hữu
    }
}
