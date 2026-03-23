using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Validators;

public static class QuizValidator
{
    public static void ValidateQuizData(int timeLimit, float totalScore, float passScore)
    {
        if (timeLimit <= 0)
            throw new ValidationException("Thời gian làm bài phải lớn hơn 0.");

        if (totalScore <= 0)
            throw new ValidationException("Tổng điểm phải lớn hơn 0.");

        if (passScore < 0)
            throw new ValidationException("Điểm đạt phải lớn hơn hoặc bằng 0.");

        if (passScore > totalScore)
            throw new ValidationException("Điểm đạt không được lớn hơn tổng điểm.");
    }
}
