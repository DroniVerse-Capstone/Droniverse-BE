using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class SubmitUserAssignmentRequestExample : IMultipleExamplesProvider<SubmitUserAssignmentRequestDTO>
{
    public IEnumerable<SwaggerExample<SubmitUserAssignmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Nộp assignment lần đầu",
            new SubmitUserAssignmentRequestDTO
            {
                MediaID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Description = "Đây là file mô tả quy trình thực hành và hình ảnh minh họa."
            }
        );

        yield return SwaggerExample.Create(
            "Nộp assignment có ghi chú",
            new SubmitUserAssignmentRequestDTO
            {
                MediaID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Description = "Tôi đã thêm phần giải thích lựa chọn waypoint trong file đính kèm."
            }
        );
    }
}

public class ReviewUserAssignmentRequestExample : IMultipleExamplesProvider<ReviewUserAssignmentRequestDTO>
{
    public IEnumerable<SwaggerExample<ReviewUserAssignmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Đạt",
            new ReviewUserAssignmentRequestDTO
            {
                Score = 85,
                ReviewComment = "Bài làm đáp ứng yêu cầu, trình bày rõ ràng và đúng quy trình."
            }
        );

        yield return SwaggerExample.Create(
            "Chưa đạt",
            new ReviewUserAssignmentRequestDTO
            {
                Score = 65,
                ReviewComment = "Cần bổ sung phần mô tả kỹ thuật và ảnh minh họa rõ hơn."
            }
        );
    }
}