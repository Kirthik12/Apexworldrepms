using ApexWorld_Backend.Features.Reports.Exceptions;
using System.Collections.Generic;
using ApexWorld_Backend.Features.Reports.DTOs;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Reports.Rules;

namespace ApexWorld_Backend.Features.Reports.Validators
{
    public class ReportRequestValidator
    {
        private readonly IEnumerable<IReportRule> _rules;

        public ReportRequestValidator(IEnumerable<IReportRule> rules)
        {
            _rules = rules;
        }

        public void Validate(ReportRequestDto request)
        {
            var errors = new List<string>();

            foreach (var rule in _rules)
            {
                var error = rule.Validate(request);
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(error);
                }
            }

            if (errors.Count > 0)
            {
                throw new ReportValidationException(errors);
            }
        }
    }
}



