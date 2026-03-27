using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Validators;

public static class TheoryValidator
{
    public static void ValidateTheoryData(int estimatedTime, string titleVN, string titleEN, string contentVN, string contentEN)
    {
        if (string.IsNullOrWhiteSpace(titleVN))
            throw new ValidationException("Tiêu đề tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(titleEN))
            throw new ValidationException("Tiêu đề tiếng Anh là bắt buộc.");

        if (string.IsNullOrWhiteSpace(contentVN))
            throw new ValidationException("Nội dung tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(contentEN))
            throw new ValidationException("Nội dung tiếng Anh là bắt buộc.");

        if (estimatedTime <= 0)
            throw new ValidationException("Thời lượng dự kiến phải lớn hơn 0.");
    }
}
