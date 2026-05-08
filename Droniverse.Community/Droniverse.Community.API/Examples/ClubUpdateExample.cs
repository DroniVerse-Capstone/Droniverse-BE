namespace Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

public class ClubUpdateExample : IExamplesProvider<ClubUpdateDto>
{
    public ClubUpdateDto GetExamples()  
    {
        return new ClubUpdateDto
        {
            NameVN = "Câu lạc bộ Drone Funny",
            NameEN = "Funny Drone Club",
            ImageMedia = Guid.Parse("3b53c998-4b6c-4cfb-b836-cd8d6cd9876e"),
            LimitParticipation = 100,
            ClubPolicyVN = "<h1>NỘI QUY CLB DRONE</h1><h2>1. Mục đích hoạt động</h2><ul><li>CLB được thành lập nhằm học tập, nghiên cứu và phát triển kỹ năng liên quan đến drone.</li><li>Tạo môi trường chia sẻ kiến thức, thực hành và sáng tạo.</li></ul>",
            ClubPolicyEN = "<h1>DRONE CLUB RULES</h1><h2>1. Purpose</h2><p>The club is established to study, research, and develop skills related to drones. It aims to create an environment for knowledge sharing, hands-on practice, and innovation.</p>",
            ClubRequirement = "Người dùng phải có kiến thức cơ bản về drone, nguyên lý hoạt động của drone"
        };
    }
}
