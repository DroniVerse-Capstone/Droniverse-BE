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
                DroneID = Guid.Parse("3aadf987-4f4f-4136-be4b-5fd906a0a9e4"),
                ClubPolicy = "Cập nhật chính sách câu lạc bộ: Tất cả thành viên phải tuân thủ quy định mới về an toàn bay.",
                Media = Guid.Parse("c25e5aa9-2777-453e-918c-fbb5859666e1"),
                NameVN = "Câu lạc bộ Drone Việt Nam (Cập nhật)",
                NameEN = "Vietnam Drone Club (Updated)",
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái. Cập nhật thông tin mới.",
                LimitParticipant = 150,
                Image = "https://images7.alphacoders.com/125/1250171.jpg"
            };
        }
    }
}
