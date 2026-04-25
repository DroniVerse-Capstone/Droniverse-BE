namespace Droniverse.Shared.Exceptions;

public class DuplicateUsernameException : BaseException
{
    public DuplicateUsernameException(string username)
        : base($"Username '{username}' already exists. Please choose another username.", "DUPLICATE_USERNAME_EXCEPTION")
    {
    }
}
