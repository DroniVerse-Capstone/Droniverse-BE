using Droniverse.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace Droniverse.Academy.Application.Validators;

public static partial class PrimitiveValidator
{
    public static void EnsureRequiredString(
        string? value,
        string requiredMessage,
        int maxLength,
        string maxLengthMessage,
        string invalidFormatMessage)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException(requiredMessage);

        EnsureMaxLength(normalized, maxLength, maxLengthMessage);
        EnsureNoBasicXss(normalized, invalidFormatMessage);
    }

    public static void EnsureOptionalString(
        string? value,
        int maxLength,
        string maxLengthMessage,
        string invalidFormatMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var normalized = value.Trim();
        EnsureMaxLength(normalized, maxLength, maxLengthMessage);
        EnsureNoBasicXss(normalized, invalidFormatMessage);
    }

    public static void EnsurePositive(int value, string message)
    {
        if (value <= 0)
            throw new ValidationException(message);
    }

    public static void EnsurePositiveNullable(int? value, string message)
    {
        if (!value.HasValue)
            return;

        if (value.Value <= 0)
            throw new ValidationException(message);
    }

    public static void EnsureNonNegative(float value, string message)
    {
        if (value < 0)
            throw new ValidationException(message);
    }

    private static void EnsureMaxLength(string value, int maxLength, string message)
    {
        if (value.Length > maxLength)
            throw new ValidationException(message);
    }

    private static void EnsureNoBasicXss(string value, string message)
    {
        if (BasicXssPattern().IsMatch(value))
            throw new ValidationException(message);
    }

    [GeneratedRegex(@"<\s*script|javascript:|vbscript:|on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BasicXssPattern();
}
