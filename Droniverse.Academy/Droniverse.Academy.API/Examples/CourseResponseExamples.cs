using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CourseDetailSuccessResponseExample : IExamplesProvider<SuccessResponse<CourseResponseDTO>>
{
    public SuccessResponse<CourseResponseDTO> GetExamples()
    {
        return SuccessResponse<CourseResponseDTO>.Create(
            new CourseResponseDTO
            {
                CourseID = Guid.Parse("9f5b91ea-b6ca-46db-a2da-18f8a6f5af13"),
                Creator = new SimpleUserReponse
                {
                    UserId = Guid.Parse("d92a8a97-4fd2-4dcf-b9fb-950f3f6a8ec8"),
                    FullName = "Nguyen Van A",
                    Email = "vana@example.com"
                },
                CreateAt = DateTime.UtcNow,
                Status = CourseStatus.DRAFT,
                MiniProduct = new ProductMiniResponseDTO
                {
                    ProductId = Guid.Parse("5f534d31-88ee-4d86-9d79-bad8f402378e"),
                    ReferenceId = Guid.Parse("7d8aa3db-7477-4af6-a06c-783f26450484"),
                    Price = 499000,
                    Currency = CurrencyType.VND,
                    Status = ProductStatus.Active
                },
                CurrentVersion = new CourseVersionResponseDTO
                {
                    CourseVersionID = Guid.Parse("7d8aa3db-7477-4af6-a06c-783f26450484"),
                    TitleVN = "Lập trình drone cơ bản",
                    TitleEN = "Drone Programming Basics",
                    DescriptionVN = "Khóa học nhập môn lập trình drone.",
                    DescriptionEN = "Introductory course for drone programming.",
                    Status = CourseVersionStatus.ACTIVE,
                    Version = 1,
                    ImageUrl = "https://cdn.example.com/course.jpg",
                    Level = CourseLevel.EASY,
                    EstimatedDuration = 120
                }
            },
            "Lấy chi tiết course thành công.");
    }
}

public class CourseOverviewSuccessResponseExample : IExamplesProvider<SuccessResponse<CourseOverviewResponseDTO>>
{
    public SuccessResponse<CourseOverviewResponseDTO> GetExamples()
    {
        return SuccessResponse<CourseOverviewResponseDTO>.Create(
            new CourseOverviewResponseDTO
            {
                CourseVersionID = Guid.Parse("7d8aa3db-7477-4af6-a06c-783f26450484"),
                TitleVN = "Lập trình drone cơ bản",
                TitleEN = "Drone Programming Basics",
                DescriptionVN = "Tổng quan khóa học.",
                DescriptionEN = "Course overview.",
                ImageUrl = "https://cdn.example.com/course.jpg",
                Level = CourseLevel.EASY,
                EstimatedDuration = 120,
                AverageRating = 4.8m,
                TotalFeedback = 45,
                TotalLearners = 320,
                TotalModules = 8,
                TotalTheory = 20,
                TotalQuiz = 8,
                TotalLab = 4,
                CertificateImageUrl = "https://cdn.example.com/certificate.jpg",
                IsUnlock = true,
                Price = 499000,
                Author = new SimpleUserReponse
                {
                    UserId = Guid.Parse("d92a8a97-4fd2-4dcf-b9fb-950f3f6a8ec8"),
                    FullName = "Nguyen Van A",
                    Email = "vana@example.com"
                },
                LastUpdatedBy = new SimpleUserReponse
                {
                    UserId = Guid.Parse("7fa6e3d0-f26b-4eaf-97e0-fc4082a4be76"),
                    FullName = "Tran Thi B",
                    Email = "thib@example.com"
                },
                LastUpdatedAt = DateTime.UtcNow
            },
            "Lấy tổng quan khóa học thành công.");
    }
}

public class CoursesByIdsSuccessResponseExample : IExamplesProvider<SuccessResponse<PagedCourseBulkResponse>>
{
    public SuccessResponse<PagedCourseBulkResponse> GetExamples()
    {
        return SuccessResponse<PagedCourseBulkResponse>.Create(
            new PagedCourseBulkResponse
            {
                TotalItems = 2,
                Items =
                [
                    new CourseBulkResponseDTO
                    {
                        CourseId = Guid.Parse("9f5b91ea-b6ca-46db-a2da-18f8a6f5af13"),
                        CourseVersionId = Guid.Parse("7d8aa3db-7477-4af6-a06c-783f26450484"),
                        TitleVN = "Lập trình drone cơ bản",
                        TitleEN = "Drone Programming Basics",
                        Level = CourseLevel.EASY,
                        EstimatedDuration = 120,
                        Price = 499000,
                        RemainingCode = 0,
                        Rating = 4.8m,
                        NumberOfParticipants = 320,
                        ImageUrl = "https://cdn.example.com/course-1.jpg"
                    },
                    new CourseBulkResponseDTO
                    {
                        CourseId = Guid.Parse("2f7a7dc5-b4e2-4d36-a014-5f85366a2d28"),
                        CourseVersionId = Guid.Parse("90f8eeb6-dafb-4ecf-a2b6-e7acf53db86f"),
                        TitleVN = "Drone nâng cao",
                        TitleEN = "Advanced Drone",
                        Level = CourseLevel.MEDIUM,
                        EstimatedDuration = 180,
                        Price = 899000,
                        RemainingCode = 0,
                        Rating = 4.6m,
                        NumberOfParticipants = 210,
                        ImageUrl = "https://cdn.example.com/course-2.jpg"
                    }
                ]
            },
            "Lấy danh sách course theo id thành công.");
    }
}
