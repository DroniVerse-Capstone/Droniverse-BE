using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Validators;

public static class LabValidator
{
    public static void ValidateLabData(int estimatedTime, string nameVN, string nameEN, string descriptionVN, string descriptionEN)
    {
        if (estimatedTime <= 0)
            throw new ValidationException("Thời lượng ước tính của lab phải lớn hơn 0.");

        if (string.IsNullOrWhiteSpace(nameVN))
            throw new ValidationException("Tên lab tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(nameEN))
            throw new ValidationException("Tên lab tiếng Anh là bắt buộc.");

        if (string.IsNullOrWhiteSpace(descriptionVN))
            throw new ValidationException("Mô tả lab tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(descriptionEN))
            throw new ValidationException("Mô tả lab tiếng Anh là bắt buộc.");
    }
}
