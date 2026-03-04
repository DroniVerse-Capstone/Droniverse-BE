namespace Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

public class ClubUpdateExample : IExamplesProvider<ClubUpdateDto>
{
    public ClubUpdateDto GetExamples()  
    {
        return new ClubUpdateDto
        {
            NameVN = "CLB Công nghệ Drone & IoT",
            NameEN = "Drone & IoT Technology Club",

            DescriptionVN = "CLB dành cho sinh viên đam mê AI, Machine Learning và Data Science. "
                          + "Tổ chức workshop, hackathon và các buổi chia sẻ kiến thức thực tế "
                          + "về ứng dụng AI trong đời sống và doanh nghiệp.",

            DescriptionEN = "A community for students passionate about Artificial Intelligence, "
                          + "Machine Learning, and Data Science. The club organizes workshops, "
                          + "hackathons, and knowledge-sharing sessions focused on real-world AI applications.",

            Status = Domain.Enums.ClubStatus.ACTIVE,
            LimitParticipation = 280,
            LimitClubManagers = 1,
            CategoryIDs = new List<Guid>
            {
                Guid.Parse("b4b420c9-d5d2-434f-a428-7edd3267dfaf"),
            }
        };
    }
}
