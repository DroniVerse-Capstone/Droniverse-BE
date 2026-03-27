using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

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
        public DateTime VisibleAt { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CompetitionStatus Status { get; set; }
        public DateTime? ResultPublishedAt { get; set; }
        public required SimpleUserReponse CreatedBy { get; set; }
        public SimpleUserReponse? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public CompetitionInvalidReason? InvalidReason { get; set; }
        public DateTime? InvalidAt { get; set; }
        public int TotalRounds { get; set; }
        public int TotalCompetitors { get; set; }
        public int TotalPrizes { get; set; }
    }
}
