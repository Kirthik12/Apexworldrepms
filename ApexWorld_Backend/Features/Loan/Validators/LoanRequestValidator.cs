using System.Collections.Generic;
using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Review.Models;
using ApexWorld_Backend.Features.Loan.DTOs;
using ApexWorld_Backend.Features.Loan.Rules;

namespace ApexWorld_Backend.Features.Loan.Validators
{
    public class LoanRequestValidator
    {
        private readonly List<ILoanRule> _rules;

        public LoanRequestValidator()
        {
            _rules = new List<ILoanRule>
            {
                new MaxReapplicationRule(),
                new ValidLoanAmountRule()
            };
        }

        public (bool IsValid, List<string> Errors) Validate(LoanApplicationRequestDto request, LoanApplication? existingApplication)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(request, existingApplication))
                {
                    errors.Add(rule.ErrorMessage);
                }
            }

            return (errors.Count == 0, errors);
        }
    }
}


