namespace Droniverse.Academy.Application.Validators;

public static class LabValidator
{
    private const int NameMaxLength = 255;
    private const int DescriptionMaxLength = 4000;

    public static void ValidateLabData(int estimatedTime, string nameVN, string nameEN, string descriptionVN, string descriptionEN)
    {
        PrimitiveValidator.EnsurePositive(estimatedTime, "Thời lượng ước tính của lab phải lớn hơn 0.");

        PrimitiveValidator.EnsureRequiredString(
            nameVN,
            "Tên lab tiếng Việt là bắt buộc.",
            NameMaxLength,
            $"Tên lab tiếng Việt không được vượt quá {NameMaxLength} ký tự.",
            "Tên lab tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            nameEN,
            "Tên lab tiếng Anh là bắt buộc.",
            NameMaxLength,
            $"Tên lab tiếng Anh không được vượt quá {NameMaxLength} ký tự.",
            "Tên lab tiếng Anh chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            descriptionVN,
            "Mô tả lab tiếng Việt là bắt buộc.",
            DescriptionMaxLength,
            $"Mô tả lab tiếng Việt không được vượt quá {DescriptionMaxLength} ký tự.",
            "Mô tả lab tiếng Việt chứa ký tự không hợp lệ.");

        PrimitiveValidator.EnsureRequiredString(
            descriptionEN,
            "Mô tả lab tiếng Anh là bắt buộc.",
            DescriptionMaxLength,
            $"Mô tả lab tiếng Anh không được vượt quá {DescriptionMaxLength} ký tự.",
            "Mô tả lab tiếng Anh chứa ký tự không hợp lệ.");
    }
}
