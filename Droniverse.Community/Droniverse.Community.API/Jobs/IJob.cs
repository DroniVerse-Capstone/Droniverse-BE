using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Jobs
{
    public interface IJob
    {
        Task ExecuteAsync();
    }
}
