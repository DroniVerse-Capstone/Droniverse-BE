using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Droniverse.Shared.Settings;

namespace Droniverse.Shared.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendRegistrationEmailAsync(
        string email,
        string fullName,
        string registrationDate,
        string confirmationUrl)
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
            _logger.LogError($"Lỗi khi gửi email đăng ký tới {email}: {ex.Message}");
            throw;
        }
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = new MailMessage
        {
            From = new MailAddress(_emailSettings.Mail, _emailSettings.DisplayName),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mail.To.Add(email);

        using var smtp = new SmtpClient(
            _emailSettings.Host,
            _emailSettings.Port)
        {
            Credentials = new NetworkCredential(
                _emailSettings.Mail,
                _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl
        };

        try
        {
            await smtp.SendMailAsync(mail);
            _logger.LogInformation($"Email gửi thành công tới {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi khi gửi email tới {email}: {ex.Message}");
            throw;
        }
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Droniverse.Shared.Templates.{templateName}";

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new FileNotFoundException($"Template không tìm thấy: {resourceName}");

            using (var reader = new StreamReader(stream))
                return await reader.ReadToEndAsync();
        }
    }
}


