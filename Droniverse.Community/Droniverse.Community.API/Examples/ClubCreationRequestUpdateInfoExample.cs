using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class ClubCreationRequestUpdateInfoExample : IExamplesProvider<ClubCreationRequestUpdateInfoDto>
    {
        public ClubCreationRequestUpdateInfoDto GetExamples()
        {
            return new ClubCreationRequestUpdateInfoDto
            {
                NameVN = "Câu lạc bộ Drone Việt Nam (Cập nhật)",
                NameEN = "Vietnam Drone Club (Updated)",
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái. Cập nhật thông tin mới.",
                IsPublic = false,
                LimitParticipant = 150,
                LimitClubManager = 3,
                Image = "https://images7.alphacoders.com/125/1250171.jpg",
                CategoryIDs = new List<Guid>
                {
                    Guid.Parse("0b27da26-062c-4ecd-8b6f-3f895d21ae4f"),
                }
            };
        }
    }
}
