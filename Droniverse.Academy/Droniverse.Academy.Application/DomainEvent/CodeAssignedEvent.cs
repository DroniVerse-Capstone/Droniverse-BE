using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DomainEvent
{
    public class CodeAssignedEvent : INotification
    {
        public string CodeId { get; }
        public Guid UserId { get; }
        public Guid CourseId { get; }

        public CodeAssignedEvent(string codeId, Guid userId, Guid courseId)
        {
            CodeId = codeId;
            UserId = userId;
            CourseId = courseId;
        }
    }
}
