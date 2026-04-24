using System;
namespace Droniverse.Community.Application.DTO.Request
{
    public class UpdateCompetitionNoLogicRequest
    {
        public DateTime VisibleAt { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
