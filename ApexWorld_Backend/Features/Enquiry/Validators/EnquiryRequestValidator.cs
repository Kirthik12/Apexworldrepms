using System.Collections.Generic;
using ApexWorld_Backend.Features.Enquiry.DTOs;
using ApexWorld_Backend.Features.Enquiry.Rules;

namespace ApexWorld_Backend.Features.Enquiry.Validators
{
    public class EnquiryRequestValidator
    {
        private readonly List<IEnquiryRule> _rules;

        public EnquiryRequestValidator()
        {
            _rules = new List<IEnquiryRule>
            {
                new ValidContactRule(),
                new ValidMessageRule()
            };
        }

        public (bool IsValid, List<string> Errors) Validate(EnquiryRequestDto request)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(request))
                {
                    errors.Add(rule.ErrorMessage);
                }
            }

            return (errors.Count == 0, errors);
        }
    }
}

