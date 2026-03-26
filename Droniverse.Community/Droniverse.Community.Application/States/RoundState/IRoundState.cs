using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.RoundState
{
    public interface IRoundState
    {
        RoundStatus Status { get; }
        void Handle(Round round, DateTime now, bool isPreviousRoundFinished);
    }
}
