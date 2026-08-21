using System;

namespace ApexWorld_Backend.Features.Audit.Exceptions
{
    public class AuditLogFailedException : Exception
    {
        public AuditLogFailedException() : base("An error occurred.") { }
        public AuditLogFailedException(string message) : base(message) { }
        public AuditLogFailedException(string message, Exception inner) : base(message, inner) { }
    }
}
