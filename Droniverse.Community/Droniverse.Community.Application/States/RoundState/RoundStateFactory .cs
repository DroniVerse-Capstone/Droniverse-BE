using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.RoundState
{
    public class RoundStateFactory : IRoundStateFactory
    {
        private readonly Dictionary<RoundStatus, IRoundState> _states;

        public RoundStateFactory(IEnumerable<IRoundState> states)
        {
            _states = states.ToDictionary(s => s.Status);
        }

        public IRoundState GetState(RoundStatus status)
        {
            if (!_states.ContainsKey(status))
                throw new InvalidOperationException($"State {status} chưa được định nghĩa.");

            return _states[status];
        }
    }
}
