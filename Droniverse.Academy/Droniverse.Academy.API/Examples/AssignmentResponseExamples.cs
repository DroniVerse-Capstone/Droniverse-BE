using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AssignmentSuccessResponseExample : IExamplesProvider<SuccessResponse<AssignmentClientViewDTO>>
{
    public SuccessResponse<AssignmentClientViewDTO> GetExamples()
    {
        return SuccessResponse<AssignmentClientViewDTO>.Create(
            new AssignmentClientViewDTO
            {
                AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TitleVN = "Bài tập lắp ráp drone cơ bản",
                TitleEN = "Basic Drone Assembly Assignment",
                DescriptionVN = "Học viên mô tả quy trình lắp ráp một drone cơ bản.",
                DescriptionEN = "Learners describe the process of assembling a basic drone.",
                Requirement = "Chụp ảnh các bước lắp ráp và nộp một file mô tả ngắn.",
                EstimatedTime = 60,
                CreateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UpdateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CreateAt = new DateTime(2026, 4, 26, 8, 30, 0, DateTimeKind.Utc),
                UpdateAt = new DateTime(2026, 4, 26, 8, 30, 0, DateTimeKind.Utc)
            },
            "Lấy assignment thành công.");
    }
}

public class AssignmentListSuccessResponseExample : IExamplesProvider<SuccessResponse<PaginationResult<IEnumerable<AssignmentClientViewDTO>>>>
{
    public SuccessResponse<PaginationResult<IEnumerable<AssignmentClientViewDTO>>> GetExamples()
    {
        return SuccessResponse<PaginationResult<IEnumerable<AssignmentClientViewDTO>>>.Create(
            new PaginationResult<IEnumerable<AssignmentClientViewDTO>>(
                [
                    new AssignmentClientViewDTO
                    {
                        AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        TitleVN = "Bài tập lắp ráp drone cơ bản",
                        TitleEN = "Basic Drone Assembly Assignment",
                        DescriptionVN = "Học viên mô tả quy trình lắp ráp một drone cơ bản.",
                        DescriptionEN = "Learners describe the process of assembling a basic drone.",
                        Requirement = "Chụp ảnh các bước lắp ráp và nộp một file mô tả ngắn.",
                        EstimatedTime = 60,
                        CreateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        UpdateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        CreateAt = new DateTime(2026, 4, 26, 8, 30, 0, DateTimeKind.Utc),
                        UpdateAt = new DateTime(2026, 4, 26, 8, 30, 0, DateTimeKind.Utc)
                    },
                    new AssignmentClientViewDTO
                    {
                        AssignmentID = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                        TitleVN = "Thiết lập waypoint bay thử nghiệm",
                        TitleEN = "Set Up a Test Flight Waypoint Plan",
                        DescriptionVN = "Xây dựng kịch bản waypoint đơn giản cho một chuyến bay thử nghiệm.",
                        DescriptionEN = "Build a simple waypoint scenario for a test flight.",
                        Requirement = "Nộp kế hoạch waypoint và giải thích lựa chọn điểm bay.",
                        EstimatedTime = 90,
                        CreateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        UpdateBy = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        CreateAt = new DateTime(2026, 4, 26, 9, 0, 0, DateTimeKind.Utc),
                        UpdateAt = new DateTime(2026, 4, 26, 9, 0, 0, DateTimeKind.Utc)
                    }
                ],
                2,
                1,
                10),
            "Lấy danh sách assignment thành công.");
    }
}

public class UserAssignmentSubmitSuccessResponseExample : IExamplesProvider<SuccessResponse<UserAssignmentSubmitResponseDTO>>
{
    public SuccessResponse<UserAssignmentSubmitResponseDTO> GetExamples()
    {
        return SuccessResponse<UserAssignmentSubmitResponseDTO>.Create(
            new UserAssignmentSubmitResponseDTO
            {
                UserAssignmentID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                EnrollmentID = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                AttemptNumber = 1,
                Status = UserAssignmentStatus.SUBMITTED,
                SubmittedAt = new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc)
            },
            "Nộp assignment thành công.");
    }
}

public class UserAssignmentReviewSuccessResponseExample : IExamplesProvider<SuccessResponse<UserAssignmentReviewResponseDTO>>
{
    public SuccessResponse<UserAssignmentReviewResponseDTO> GetExamples()
    {
        return SuccessResponse<UserAssignmentReviewResponseDTO>.Create(
            new UserAssignmentReviewResponseDTO
            {
                UserAssignmentID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                EnrollmentID = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Score = 85,
                IsPassed = true,
                Status = UserAssignmentStatus.PASSED,
                ReviewComment = "Bài làm đáp ứng yêu cầu, trình bày rõ ràng và đúng quy trình.",
                ReviewedBy = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                ReviewedAt = new DateTime(2026, 4, 26, 11, 0, 0, DateTimeKind.Utc)
            },
            "Chấm assignment thành công.");
    }
}

public class UserAssignmentAttemptsSuccessResponseExample : IExamplesProvider<SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>>
{
    public SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetExamples()
    {
        return SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>.Create(
            new PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>(
                [
                    new UserAssignmentAttemptResponseDTO
                    {
                        UserAssignmentID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                        AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        EnrollmentID = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                        AttemptNumber = 1,
                        MediaID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Description = "Đây là file mô tả quy trình thực hành và hình ảnh minh họa.",
                        Status = UserAssignmentStatus.SUBMITTED,
                        Score = null,
                        ReviewComment = null,
                        ReviewedBy = null,
                        ReviewedAt = null,
                        SubmittedAt = new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc)
                    },
                    new UserAssignmentAttemptResponseDTO
                    {
                        UserAssignmentID = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        AssignmentID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        EnrollmentID = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                        AttemptNumber = 2,
                        MediaID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Description = "Tôi đã thêm phần giải thích lựa chọn waypoint trong file đính kèm.",
                        Status = UserAssignmentStatus.PASSED,
                        Score = 85,
                        ReviewComment = "Bài làm đáp ứng yêu cầu, trình bày rõ ràng và đúng quy trình.",
                        ReviewedBy = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        ReviewedAt = new DateTime(2026, 4, 26, 11, 0, 0, DateTimeKind.Utc),
                        SubmittedAt = new DateTime(2026, 4, 26, 10, 30, 0, DateTimeKind.Utc)
                    }
                ],
                2,
                1,
                10),
            "Lấy lịch sử nộp assignment thành công.");
    }
}