using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace Droniverse.Shared.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _env = environment;
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
        var emailSettings = _configuration.GetSection("EmailSettings");

        var mail = new MailMessage
        {
            From = new MailAddress(
                emailSettings["Mail"],
                emailSettings["DisplayName"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mail.To.Add(email);

        using var smtp = new SmtpClient(
            emailSettings["Host"],
            int.Parse(emailSettings["Port"]))
        {
            Credentials = new NetworkCredential(
                emailSettings["Mail"],
                emailSettings["Password"]),
            EnableSsl = true,
            Timeout = 30000  // 30 giây
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


