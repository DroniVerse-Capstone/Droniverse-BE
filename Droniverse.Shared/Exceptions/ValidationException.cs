namespace Droniverse.Shared.Exceptions;

public class ValidationException : BaseException
{
    public ValidationException(string message) : base(message, "VALIDATION_EXCEPTION")
    {
    }
}
