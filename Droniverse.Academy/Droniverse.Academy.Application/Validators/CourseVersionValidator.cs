using Droniverse.Academy.Application.DTO.Request;

namespace Droniverse.Academy.Application.Validators;

public static class CourseVersionValidator
{
    private const int TitleMaxLength = 255;
    private const int DescriptionMaxLength = 4000;
    private const int ContextMaxLength = 8000;
    private const int ChangeLogMaxLength = 2000;
    private const int ImageUrlMaxLength = 2000;

    public static void ValidateCreateData(CreateCourseVersionRequestDTO request)
    {
        ValidateCommonData(
            request.TitleVN,
            request.TitleEN,
            request.DescriptionVN,
            request.DescriptionEN,
            request.ContextVN,
            request.ContextEN,
            request.ImageUrl,
            request.EstimatedDuration,
            request.ChangeLog);
    }

    public static void ValidateUpdateData(UpdateCourseVersionRequestDTO request)
    {
        ValidateCommonData(
            request.TitleVN,
            request.TitleEN,
            request.DescriptionVN,
            request.DescriptionEN,
            request.ContextVN,
            request.ContextEN,
            request.ImageUrl,
            request.EstimatedDuration,
            request.ChangeLog);
    }

    private static void ValidateCommonData(
        string? titleVN,
        string? titleEN,
        string? descriptionVN,
        string? descriptionEN,
        string? contextVN,
        string? contextEN,
        string? imageUrl,
        int? estimatedDuration,
        string? changeLog)
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

        PrimitiveValidator.EnsureOptionalString(
            descriptionVN,
            DescriptionMaxLength,
            $"Mô tả tiếng Việt không được vượt quá {DescriptionMaxLength} ký tự.",
            "Mô tả tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureOptionalString(
            descriptionEN,
            DescriptionMaxLength,
            $"Mô tả tiếng Anh không được vượt quá {DescriptionMaxLength} ký tự.",
            "Mô tả tiếng Anh chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureOptionalString(
            contextVN,
            ContextMaxLength,
            $"Context tiếng Việt không được vượt quá {ContextMaxLength} ký tự.",
            "Context tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureOptionalString(
            contextEN,
            ContextMaxLength,
            $"Context tiếng Anh không được vượt quá {ContextMaxLength} ký tự.",
            "Context tiếng Anh chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureOptionalString(
            imageUrl,
            ImageUrlMaxLength,
            $"ImageUrl không được vượt quá {ImageUrlMaxLength} ký tự.",
            "ImageUrl chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureOptionalString(
            changeLog,
            ChangeLogMaxLength,
            $"ChangeLog không được vượt quá {ChangeLogMaxLength} ký tự.",
            "ChangeLog chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsurePositiveNullable(
            estimatedDuration,
            "Thời lượng dự kiến phải lớn hơn 0.");
    }
}
