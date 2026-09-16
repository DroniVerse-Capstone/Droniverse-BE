using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using Microsoft.AspNetCore.Mvc;


namespace Droniverse.Community.Application.DTO.Extensions
{
    public class ParticipationSearchRequest : SearchRequest
    {
        public string? ParticipantName { get; set; }
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

    public class HotCompetitionSearchRequest : SearchRequest
    {
    }

    public class CompetitionLeaderboardSearchRequest : SearchRequest
    {
    }

    public class RoundParticipantsSearchRequest : SearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
        public string? SeachUserName
        {
            get => SearchName;
            set => SearchName = value;
        }
        public DateTime? ParticipantStartedFrom { get; set; }
        public DateTime? ParticipantStartedEnd { get; set; }
        public DateTime? ParticipationSubmittedFrom { get; set; }
        public DateTime? ParticipationSubmittedEnd { get; set; }
        public UserRoundStatus? ParticipantStatus { get; set; }
        public bool? IsPassed { get; set; }
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

    public class RoundLeaderboardSearchRequest : SearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; }
    }

    public class UserInfoSearchRequest : SearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
    }

    public class GetAllClubPoliciesSearchRequest : SearchRequest
    {

    }

    public class GetUserPrizeCurrentUserSearchRequest : SearchRequest
    {
        public string? CompetitionName { get; set; }
    }

    public class GetAllClubsSearchRequest : SearchRequest
    {
        public string? ClubName { get; set; }
        public ClubStatus? ClubStatus { get; set; }
    }

    public class RoundResultAllParicipations : SearchRequest
    {
        public UserRoundStatus? Status { get; set; }
        public RoundResultAllSortBy SortBy { get; set; } = RoundResultAllSortBy.StartedAt;
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }

    public class MyRoundSearchRequest : SearchRequest
    {
        public UserRoundStatus? UserRoundStatus { get; set; }
        public RoundStatus? RoundStatus { get; set; }
        public bool? IsPassed { get; set; }
    }

    public class CompetitionParticipantsSearchRequest : SearchRequest
    {
        public UserCompetitionStatus Status { get; set; }
        public DateTime? JoinFrom { get; set; }
    }

}
