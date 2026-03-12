using Droniverse.Shared.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Infrastructure.Common
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
