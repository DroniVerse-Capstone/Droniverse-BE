using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class CompetitionResponse
    {
        public Guid CompetitionID { get; set; }
        public Guid ClubID { get; set; }
        public required string NameVN { get; set; }
        public required string NameEN { get; set; }
        public string? DescriptionVN { get; set; }
        public string? DescriptionEN { get; set; }
        public required string RuleContent { get; set; }
        public int? MaxParticipants { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CompetitionStatus Status { get; set; }
        public DateTime? ResultPublishedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int totalRounds { get; set; }
        public int totalCompetitors { get; set; }
        public int totalPrizes  { get; set; }
    }
}
