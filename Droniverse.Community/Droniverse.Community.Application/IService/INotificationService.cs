using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface INotificationService
    {
        Task SendCompetitionInvalidEmailAsync(Competition competition);
    }
}
