using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class EmailNotificationService : INotificationService
    {
        public Task SendCompetitionInvalidEmailAsync(Competition competition)
        {
            // gửi mail qua SMTP / SendGrid / v.v.
            Console.WriteLine($"[EMAIL] Competition {competition.CompetitionID} INVALID: {competition.InvalidReason}");
            return Task.CompletedTask;
        }
    }
}
