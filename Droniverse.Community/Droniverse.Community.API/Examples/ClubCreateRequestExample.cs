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
                DroneID = Guid.Parse("30876cff-7818-44ab-8fee-bc2b99c36e7c"),
                Media = Guid.Parse("0105131f-f0bd-46f9-abef-ecc64f88f4cc"),
                ClubPolicy = "Nội quy của câu lạc bộ VN",
                NameVN = "Câu lạc bộ Drone Việt Nam",
                NameEN = "Vietnam Drone Club",
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái.",
                LimitParticipant = 200,
                Image = "https://res.cloudinary.com/ds9f2jnnj/image/upload/v1775040967/droniverse/temp/Anh_dai_dien_fd6e47c316_d3vidp.jpg"

            };
        }
    }
}
