using MediatR;

namespace Droniverse.Academy.Application.DomainEvent
{
    public class CodeAssignedEvent : INotification
    {
        public string Code { get; }
        public Guid UserId { get; }
        public Guid CourseId { get; }
        public string Email { get; }

        public string FullName { get; }
        public string CourseNameVN { get; }
        public string CourseNameEN { get; }

        public CodeAssignedEvent(
            string code,
            Guid userId,
            Guid courseId,
            string email,
            string fullName,
            string courseNameVN,
            string courseNameEN)
        {
            Code = code;
            UserId = userId;
            CourseId = courseId;
            Email = email;
            FullName = fullName;
            CourseNameVN = courseNameVN;
            CourseNameEN = courseNameEN;
        }
    }
}