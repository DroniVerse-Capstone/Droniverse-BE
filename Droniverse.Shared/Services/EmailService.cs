using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Droniverse.Shared.Settings;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Shared.Services;

public class EmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SendGridSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(
        string email, string userName, 
        string orderId, string orderDate, 
        string productId, string productNameVN, string productNameEN, string type, decimal unitOfPrice, int quantity, decimal totalAmount)
    {
        try
        {
            string htmlContent = await LoadTemplateAsync("OrderConfirmationTemplate.html");
            htmlContent = htmlContent
                .Replace("{Email}", email)
                .Replace("{UserName}", userName)
                .Replace("{OrderId}", orderId)
                .Replace("{OrderDate}", orderDate)
                .Replace("{ProductId}", productId)
                .Replace("{ProductNameVN}", productNameVN)
                .Replace("{ProductNameEN}", productNameEN)
                .Replace("{Type}", type)
                .Replace("{UnitOfPrice}", unitOfPrice.ToString("N0"))
                .Replace("{Quantity}", quantity.ToString())
                .Replace("{TotalAmount}", totalAmount.ToString("N0"));
            await SendEmailAsync(email, "Xác nhận đơn hàng của bạn tại Droniverse!", htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi: {ex.Message}");
            throw;
        }
    }

    public async Task SendRegistrationEmailAsync(
        string email, string fullName, string registrationDate, string confirmationUrl)
    {
        try
        {
            string htmlContent = await LoadTemplateAsync("RegisterTemplate.html");
            htmlContent = htmlContent
                .Replace("{Email}", email)
                .Replace("{FullName}", fullName)
                .Replace("{RegistrationDate}", registrationDate)
                .Replace("{ConfirmationUrl}", confirmationUrl);
            await SendEmailAsync(email, "Chào mừng bạn đến với Droniverse!", htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi: {ex.Message}");
            throw;
        }
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SendGridClient(_settings.ApiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            HtmlContent = message
        };
        msg.AddTo(email);

        await client.SendEmailAsync(msg);
        _logger.LogInformation($"Email sent to {email}");
    }
        
    public async Task<string> LoadTemplateAsync(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Droniverse.Shared.Templates.{templateName}";
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) throw new FileNotFoundException($"Template không tìm thấy: {resourceName}");
            using (var reader = new StreamReader(stream))
                return await reader.ReadToEndAsync();
        }
    }
}


