using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public interface ICompetitionStateFactory
    {
        ICompetitionState GetState(CompetitionStatus status);
    }
}
