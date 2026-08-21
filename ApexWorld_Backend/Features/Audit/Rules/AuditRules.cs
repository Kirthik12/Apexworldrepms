namespace ApexWorld_Backend.Features.Audit.Rules
{
    public interface IAuditRule
    {
        bool IsSatisfiedBy(string action, string entityType);
        string ErrorMessage { get; }
    }

    public class ValidAuditDetailsRule : IAuditRule
    {
        public string ErrorMessage => "Action and EntityType cannot be empty.";

        public bool IsSatisfiedBy(string action, string entityType)
        {
            return !string.IsNullOrWhiteSpace(action) && !string.IsNullOrWhiteSpace(entityType);
        }
    }
}

