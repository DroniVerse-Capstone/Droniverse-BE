using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Validators;

public static class QuizValidator
{
    public static void ValidateQuizData(int timeLimit, float totalScore, float passScore)
    {
        PrimitiveValidator.EnsurePositive(timeLimit, "Thời gian làm bài phải lớn hơn 0.");

        if (totalScore <= 0)
            throw new ValidationException("Tổng điểm phải lớn hơn 0.");

        PrimitiveValidator.EnsureNonNegative(passScore, "Điểm đạt phải lớn hơn hoặc bằng 0.");

        if (passScore > totalScore)
            throw new ValidationException("Điểm đạt không được lớn hơn tổng điểm.");
    }
}
