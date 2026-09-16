using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateQuizQuestionRequestExample : IMultipleExamplesProvider<UpdateQuizQuestionRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateQuizQuestionRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật câu hỏi quiz",
            new UpdateQuizQuestionRequestDTO
            {
                ContentVN = "Khi mất tín hiệu điều khiển, drone nên thực hiện hành động nào?",
                ContentEN = "When control signal is lost, what should the drone do?",
                AnswerA = "Tăng tốc tối đa",
                AnswerB = "Bay ngẫu nhiên",
                AnswerC = "Kích hoạt chế độ quay về điểm xuất phát",
                AnswerD = "Tắt động cơ ngay lập tức",
                AnswerA_EN = "Increase speed to maximum",
                AnswerB_EN = "Fly randomly",
                AnswerC_EN = "Activate return-to-home mode",
                AnswerD_EN = "Shut down motors immediately",
                CorrectAnswer = "C",
                Score = 1
            }
        );
    }
}
