using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public new string ErrorCode { get; init; }

        public ForbiddenException(string message, string errorCode = "FORBIDDEN") : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
