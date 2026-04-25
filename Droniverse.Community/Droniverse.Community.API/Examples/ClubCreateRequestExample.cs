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
                DroneID = Guid.Parse("d6a77e29-aa62-4928-94ec-5590c958f7b1"),
                Media = Guid.Parse("27447695-ba40-438f-8afa-d74c08a901f8"),
                ClubPolicyVN = "Dưới đây là nội dung đoạn văn bản trong hình:\r\n\r\nI. CƠ CHẾ GIA NHẬP\r\n\r\n1. Đối tượng tham gia:\r\n\r\nCâu lạc bộ chỉ tuyển chọn 5% tổng số sinh viên trên một chi đoàn để đảm bảo chất lượng và sự tập trung trong hoạt động.\r\n\r\nƯu tiên những bạn có chí cầu tiến, sự quan tâm thực sự đến mục đích và hoạt động của câu lạc bộ.\r\n\r\nYêu cầu GPA từ loại giỏi trở lên (hoặc tương đương) để đảm bảo thành viên có nền tảng học tập vững chắc.",
                ClubPolicyEN = "I. ADMISSION MECHANISM\r\n\r\n1. Eligible participants:\r\n\r\nThe club only recruits 5% of the total students per youth union branch to ensure quality and focus in its activities.\r\n\r\nPriority is given to those who are driven and have a genuine interest in the club's purpose and activities.\r\n\r\nA GPA of \"Excellent\" (Giỏi) or above (or equivalent) is required to ensure members have a solid academic foundation.",
                ClubRequirement = "Ứng viên cần nêu rõ kinh nghiệm bay drone hoặc mục tiêu học tập khi tham gia câu lạc bộ.",
                NameVN = "Câu lạc bộ Drone Việt Nam",
                NameEN = "Vietnam Drone Club",
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái.",
                LimitParticipant = 200,
                Image = "https://res.cloudinary.com/ds9f2jnnj/image/upload/v1775040967/droniverse/temp/Anh_dai_dien_fd6e47c316_d3vidp.jpg"

            };
        }
    }
}
