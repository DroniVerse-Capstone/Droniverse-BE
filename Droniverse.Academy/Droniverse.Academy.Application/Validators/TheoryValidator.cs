namespace Droniverse.Academy.Application.Validators;

public static class TheoryValidator
{
    private const int TitleMaxLength = 255;
    private const int ContentMaxLength = 20000;

    public static void ValidateTheoryData(int estimatedTime, string titleVN, string titleEN, string contentVN, string contentEN)
    {
        PrimitiveValidator.EnsureRequiredString(
            titleVN,
            "Tiêu đề tiếng Việt là bắt buộc.",
            TitleMaxLength,
            $"Tiêu đề tiếng Việt không được vượt quá {TitleMaxLength} ký tự.",
            "Tiêu đề tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            titleEN,
            "Tiêu đề tiếng Anh là bắt buộc.",
            TitleMaxLength,
            $"Tiêu đề tiếng Anh không được vượt quá {TitleMaxLength} ký tự.",
            "Tiêu đề tiếng Anh chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            contentVN,
            "Nội dung tiếng Việt là bắt buộc.",
            ContentMaxLength,
            $"Nội dung tiếng Việt không được vượt quá {ContentMaxLength} ký tự.",
            "Nội dung tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            contentEN,
            "Nội dung tiếng Anh là bắt buộc.",
            ContentMaxLength,
            $"Nội dung tiếng Anh không được vượt quá {ContentMaxLength} ký tự.",
            "Nội dung tiếng Anh chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsurePositive(estimatedTime, "Thời lượng dự kiến phải lớn hơn 0.");
    }
}
