namespace Droniverse.Shared.Services;

public interface IEmailService
{
    Task SendRegistrationEmailAsync(
        string email,
        string fullName,
        string registrationDate,
        string confirmationUrl);

    Task SendEmailAsync(string email, string subject, string message);
}

