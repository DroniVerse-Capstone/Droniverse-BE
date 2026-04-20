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

                ClubPolicy = new ClubPolicyCreateDto
                {
                    Title = "Nội quy của câu lạc bộ",
                    Content = "Đây là nội dung của nội quy câu lạc bộ. Nó nêu rõ các quy tắc và hướng dẫn cho các thành viên câu lạc bộ."
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

                ClubPolicy = new ClubPolicyCreateDto
                {
                    Title = "Nội quy của câu lạc bộ",
                    Content = "Đây là nội dung của nội quy câu lạc bộ. Nó nêu rõ các quy tắc và hướng dẫn cho các thành viên câu lạc bộ."
                }
            });
    }
}