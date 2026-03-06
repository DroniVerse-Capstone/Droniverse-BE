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
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái.",
                IsPublic = true,
                LimitParticipant = 200,
                LimitClubManager = 5,
                Image = "https://bom.edu.vn/public/upload/2024/12/avatar-goku-cute-1.webp",
                CategoryIDs = new List<Guid>
                {
                    Guid.Parse("0b27da26-062c-4ecd-8b6f-3f895d21ae4f"),
                    Guid.Parse("44ca2075-50fd-4b8f-a60c-4db6ad7cc708")
                }
            };
        }
    }
}
