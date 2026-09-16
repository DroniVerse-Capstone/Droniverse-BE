

namespace Droniverse.Shared.Exceptions;

    public class DomainException : Exception
    {
        public string ErrorCode { get; }

        public DomainException(string message, string errorCode = "DOMAIN_ERROR")
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
