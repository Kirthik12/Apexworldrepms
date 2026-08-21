using System.Collections.Generic;
using ApexWorld_Backend.Features.Audit.Rules;

namespace ApexWorld_Backend.Features.Audit.Validators
{
    public class AuditRequestValidator
    {
        private readonly List<IAuditRule> _rules;

        public AuditRequestValidator()
        {
            _rules = new List<IAuditRule>
            {
                new ValidAuditDetailsRule()
            };
        }

        public (bool IsValid, List<string> Errors) Validate(string action, string entityType)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(action, entityType))
                {
                    errors.Add(rule.ErrorMessage);
                }
            }

            return (errors.Count == 0, errors);
        }
    }
}

