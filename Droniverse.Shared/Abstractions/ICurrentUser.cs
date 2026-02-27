using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.Abstractions
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
