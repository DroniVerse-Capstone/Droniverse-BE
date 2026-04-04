using Droniverse.Shared.Settings;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Reflection;

namespace Droniverse.Shared.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IOptions<EmailSettings> _emailSettings;
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 2000;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment,
        IOptions<EmailSettings> emailSettings)
    {
        _configuration = configuration;
        _env = environment;
        _logger = logger;
        _emailSettings = emailSettings;
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

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await SendEmailInternalAsync(email, subject, message, emailSettings);
                _logger.LogInformation($"Email gửi thành công tới {email} (attempt {attempt})");
                return;
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning($"Timeout lần {attempt}/{MaxRetries} khi gửi email tới {email}: {ex.Message}");

                if (attempt < MaxRetries)
                {
                    int delayMs = BaseDelayMs * attempt; // Exponential backoff: 2s, 4s, 6s
                    _logger.LogInformation($"Retry sau {delayMs}ms...");
                    await Task.Delay(delayMs);
                }
                else
                {
                    _logger.LogError($"Gửi email thất bại sau {MaxRetries} lần retry tới {email}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gửi email tới {email}: {ex.Message}");
                throw;
            }
        }
    }

    private async Task SendEmailInternalAsync(
        string email, 
        string subject, 
        string message,
        IConfigurationSection emailSettings)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(emailSettings["DisplayName"], emailSettings["Mail"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using (var client = new SmtpClient())
        {
            // Timeout: connect 15s, send 30s
            client.Timeout = 30000;

            await client.ConnectAsync(
                emailSettings["Host"], 
                int.Parse(emailSettings["Port"]), 
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(emailSettings["Mail"], emailSettings["Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
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


