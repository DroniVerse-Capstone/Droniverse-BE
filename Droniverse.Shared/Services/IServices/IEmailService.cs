namespace Droniverse.Shared.Services.IServices;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(
        string email, string userName,
        string orderId, string orderDate,
        string productId, string productNameVN,
        string productNameEN, string type,
        decimal unitOfPrice, int quantity,
        decimal totalAmount);
    Task SendRegistrationEmailAsync(
        string email,
        string fullName,
        string registrationDate,
        string confirmationUrl);

    Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl,
        string verificationToken);

    Task SendEmailAsync(string email, string subject, string message);

    Task<string> LoadTemplateAsync(string templateName);

}

