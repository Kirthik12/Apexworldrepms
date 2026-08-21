using ApexWorld_Backend.Features.Review.Exceptions;
using System.Collections.Generic;
using ApexWorld_Backend.Features.Review.DTOs;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Review.Rules;

namespace ApexWorld_Backend.Features.Review.Validators
{
    public class PlatformReviewValidator
    {
        private readonly IEnumerable<IReviewRule<CreatePlatformReviewDto>> _rules;

        public PlatformReviewValidator(IEnumerable<IReviewRule<CreatePlatformReviewDto>> rules)
        {
            _rules = rules;
        }

        public void Validate(CreatePlatformReviewDto request)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                var error = rule.Validate(request);
                if (error != null) errors.Add(error);
            }
            if (errors.Count > 0) throw new ReviewValidationException(errors);
        }
    }

    public class PropertyReviewValidator
    {
        private readonly IEnumerable<IReviewRule<CreatePropertyReviewDto>> _rules;

        public PropertyReviewValidator(IEnumerable<IReviewRule<CreatePropertyReviewDto>> rules)
        {
            _rules = rules;
        }

        public void Validate(CreatePropertyReviewDto request)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                var error = rule.Validate(request);
                if (error != null) errors.Add(error);
            }
            if (errors.Count > 0) throw new ReviewValidationException(errors);
        }
    }
}



