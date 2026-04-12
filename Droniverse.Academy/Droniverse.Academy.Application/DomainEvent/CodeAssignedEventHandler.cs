using Droniverse.Shared.Services.IServices;
using MediatR;

namespace Droniverse.Academy.Application.DomainEvent
{
    public class CodeAssignedEventHandler : INotificationHandler<CodeAssignedEvent>
    {
        private readonly IEmailService _emailService;

        public CodeAssignedEventHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(
            CodeAssignedEvent notification,
            CancellationToken cancellationToken)
        {
            //await _emailService.SendEmailAsync(
            //    notification.UserId,
            //    notification.CodeId,
            //    notification.CourseId
            //);
        }
    }

}

