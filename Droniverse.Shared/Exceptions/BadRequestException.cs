namespace Droniverse.Shared.Exceptions;

public class BadRequestException : BaseException
{
    public BadRequestException(string message) : base(message, "BAD_REQUEST_EXCEPTION")
    {
    }
}
