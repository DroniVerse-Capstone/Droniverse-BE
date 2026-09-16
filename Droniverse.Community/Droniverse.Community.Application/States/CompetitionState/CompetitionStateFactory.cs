using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class CompetitionStateFactory : ICompetitionStateFactory
    {
        private readonly Dictionary<CompetitionStatus, ICompetitionState> _states;

        public CompetitionStateFactory(IEnumerable<ICompetitionState> states)
        {
            _states = states.ToDictionary(s => s.Status);
        }

        public ICompetitionState GetState(CompetitionStatus status)
        {
            if (!_states.ContainsKey(status))
                throw new InvalidOperationException($"State {status} chưa được định nghĩa.");

            return _states[status];
        }
    }
}
