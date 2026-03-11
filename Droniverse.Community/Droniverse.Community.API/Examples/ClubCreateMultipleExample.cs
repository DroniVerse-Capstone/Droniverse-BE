using Swashbuckle.AspNetCore.Filters;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.API.Examples;

public class ClubCreateMultipleExample
    : IMultipleExamplesProvider<ClubCreateDto>
{
    public IEnumerable<SwaggerExample<ClubCreateDto>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Public Drone Technology Club",
            "A public club focused on drone innovation",
            new ClubCreateDto
            {
                NameVN = "CLB Công Nghệ Drone",
                NameEN = "Drone Technology Club",

                DescriptionVN = "Câu lạc bộ nghiên cứu, phát triển và ứng dụng công nghệ drone trong nông nghiệp, giám sát và logistics.",
                DescriptionEN = "A club dedicated to researching, developing, and applying drone technology in agriculture, surveillance, and logistics.",

                Status = ClubStatus.ACTIVE,
                IsPublic = true,

                LimitParticipation = 300,
                LimitClubManagers = 5,
                CategoryIDs = new List<Guid>
                {
                    Guid.Parse("d6cd6805-a231-4661-ad66-0b3382763a81"),
                }
            });

        yield return SwaggerExample.Create(
            "Private AI Research Group",
            "Internal AI & Robotics research club",
            new ClubCreateDto
            {
                NameVN = "CLB Nghiên Cứu AI & Robotics",
                NameEN = "AI & Robotics Research Club",

                DescriptionVN = "Nhóm nghiên cứu chuyên sâu về trí tuệ nhân tạo, robot tự hành và thị giác máy tính.",
                DescriptionEN = "An advanced research group focusing on AI, autonomous robotics, and computer vision.",

                Status = ClubStatus.ACTIVE,
                IsPublic = false,

                LimitParticipation = 80,
                LimitClubManagers = 3,

                CategoryIDs = new List<Guid>
                {
                    Guid.Parse("d6cd6805-a231-4661-ad66-0b3382763a81"),
                    Guid.Parse("b4b420c9-d5d2-434f-a428-7edd3267dfaf")
                }
            });
    }
}