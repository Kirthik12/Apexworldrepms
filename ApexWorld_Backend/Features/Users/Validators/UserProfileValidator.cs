using ApexWorld_Backend.Features.Users.Exceptions;
using System.Collections.Generic;
using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Users.Rules;

namespace ApexWorld_Backend.Features.Users.Validators
{
    public class UpdateBuyerProfileValidator
    {
        private readonly IEnumerable<IUserProfileRule<UpdateBuyerProfileDto>> _rules;

        public UpdateBuyerProfileValidator(IEnumerable<IUserProfileRule<UpdateBuyerProfileDto>> rules)
        {
            _rules = rules;
        }

        public void Validate(UpdateBuyerProfileDto request)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                var error = rule.Validate(request);
                if (error != null) errors.Add(error);
            }
            if (errors.Count > 0) throw new AuthValidationException(errors); // Re-using AuthValidationException or create a new one
        }


    }

    public class UpdateAdminProfileValidator
    {
        private readonly IEnumerable<IUserProfileRule<UpdateAdminProfileDto>> _rules;

        public UpdateAdminProfileValidator(IEnumerable<IUserProfileRule<UpdateAdminProfileDto>> rules)
        {
            _rules = rules;
        }

        public void Validate(UpdateAdminProfileDto request)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                var error = rule.Validate(request);
                if (error != null) errors.Add(error);
            }
            if (errors.Count > 0) throw new AuthValidationException(errors);
        }
    }
}



