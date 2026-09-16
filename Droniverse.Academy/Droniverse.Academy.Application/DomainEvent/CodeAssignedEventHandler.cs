using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using MediatR;

namespace Droniverse.Academy.Application.DomainEvent
{
    public class CodeAssignedEventHandler : INotificationHandler<CodeAssignedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IClock _clock;

        public CodeAssignedEventHandler(IEmailService emailService, IClock clock)
        {
            _emailService = emailService;
            _clock = clock;
        }

        public async Task Handle(CodeAssignedEvent notification, CancellationToken cancellationToken)
        {
            //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SendCodeCourseTemplate.html");

            var html = await _emailService.LoadTemplateAsync("SendCodeCourseTemplate.html");

            html = html.Replace("{FullName}", notification.FullName)
                       .Replace("{Email}", notification.Email)
                       .Replace("{CourseNameVN}", notification.CourseNameVN)
                       .Replace("{CourseNameEN}", notification.CourseNameEN)
                       .Replace("{Code}", notification.Code)
                       .Replace("{Date}", _clock.Now.TrimToMinute().ToString());

            var subject = $"[Droniverse] Bạn đã được cấp mã khóa học {notification.CourseNameEN}";

            await _emailService.SendEmailAsync(
                notification.Email,
                subject,
                html
            );
        }
    }

}

