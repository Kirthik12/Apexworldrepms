using System.Collections.Generic;
using ApexWorld_Backend.Features.Booking.DTOs;
using ApexWorld_Backend.Features.Booking.Rules;

namespace ApexWorld_Backend.Features.Booking.Validators
{
    public class BookingRequestValidator
    {
        private readonly List<IBookingRule> _rules;

        public BookingRequestValidator()
        {
            _rules = new List<IBookingRule>();
        }

        public (bool IsValid, List<string> Errors) Validate(BookingRequestDto request)
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

