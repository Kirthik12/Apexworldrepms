using ApexWorld_Backend.Features.Booking.DTOs;
using System;

namespace ApexWorld_Backend.Features.Booking.Rules
{
    public interface IBookingRule
    {
        bool IsSatisfiedBy(BookingRequestDto request);
        string ErrorMessage { get; }
    }

}

