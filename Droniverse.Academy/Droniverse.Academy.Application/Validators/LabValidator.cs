using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Validators;

public static class LabValidator
{
    public static void ValidateLabData(string nameVN, string nameEN, string descriptionVN, string descriptionEN)
    {
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
