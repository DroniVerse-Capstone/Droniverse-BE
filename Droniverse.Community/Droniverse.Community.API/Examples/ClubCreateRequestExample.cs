using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class ClubCreateRequestExample : IExamplesProvider<ClubCreationRequestCreateDto>
    {
        public ClubCreationRequestCreateDto GetExamples()
        {
            return new ClubCreationRequestCreateDto
            {
                NameVN = "Câu lạc bộ Drone Việt Nam",
                NameEN = "Vietnam Drone Club",
                DescriptionVN = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái.",
                DescriptionEN = "A club for people passionate about drones and UAV technology.",
                ClubCode = "DRONE1",
                IsPublic = true,
                LimitParticipant = 200,
                LimitClubManager = 5,

                // File upload không thể set example thật
                Image = null
            };
        }
    }
}
