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
                DroneID = Guid.Parse("fcb4cfcb-1928-42ca-a5e9-1cd7f0d340f3"),
                ClubPolicyVN = "(Cập nhật). Dưới đây là nội dung đoạn văn bản trong hình:\r\n\r\nI. CƠ CHẾ GIA NHẬP\r\n\r\n1. Đối tượng tham gia:\r\n\r\nCâu lạc bộ chỉ tuyển chọn 5% tổng số sinh viên trên một chi đoàn để đảm bảo chất lượng và sự tập trung trong hoạt động.\r\n\r\nƯu tiên những bạn có chí cầu tiến, sự quan tâm thực sự đến mục đích và hoạt động của câu lạc bộ.\r\n\r\nYêu cầu GPA từ loại giỏi trở lên (hoặc tương đương) để đảm bảo thành viên có nền tảng học tập vững chắc.",
                ClubPolicyEN = "(Updated) I. ADMISSION MECHANISM\r\n\r\n1. Eligible participants:\r\n\r\nThe club only recruits 5% of the total students per youth union branch to ensure quality and focus in its activities.\r\n\r\nPriority is given to those who are driven and have a genuine interest in the club's purpose and activities.\r\n\r\nA GPA of \"Excellent\" (Giỏi) or above (or equivalent) is required to ensure members have a solid academic foundation.",
                ClubRequirement = "(Cập nhật) Thành viên cần cam kết tham gia tối thiểu 2 buổi sinh hoạt/tháng.",
                Media = Guid.Parse("f0247ddd-521d-405f-8344-f246b23d2d1d"),
                NameVN = "Câu lạc bộ Drone Việt Nam (Cập nhật)",
                NameEN = "Vietnam Drone Club (Updated)",
                Description = "Câu lạc bộ dành cho những người yêu thích drone và công nghệ bay không người lái. Cập nhật thông tin mới.",
                LimitParticipant = 150,
                Image = "https://images7.alphacoders.com/125/1250171.jpg"
            };
        }
    }
}
