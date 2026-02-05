namespace Droniverse.Shared.Exceptions;

public class DuplicateEmailException : BaseException
{
    public string ErrorCode { get; set; }
    public DuplicateEmailException(string email) : base($"This email {email} has been used.")
    {
        ErrorCode = "DUPLICATE_EMAIL_EXCEPTION";
    }
}
