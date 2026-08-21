using ApexWorld_Backend.Features.Property.Exceptions;
using System.Collections.Generic;
using ApexWorld_Backend.Features.Property.DTOs;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Property.Rules;

namespace ApexWorld_Backend.Features.Property.Validators
{
    public class PropertyRequestValidator
    {
        private readonly IEnumerable<IPropertyRule> _rules;

        public PropertyRequestValidator(IEnumerable<IPropertyRule> rules)
        {
            _rules = rules;
        }

        public void Validate(PropertyCreateDto request)
        {
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(request))
                {
                    throw new PropertyValidationException(rule.ErrorMessage);
                }
            }
        }
    }
}



