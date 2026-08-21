using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Review.Models;
using ApexWorld_Backend.Features.Loan.DTOs;

namespace ApexWorld_Backend.Features.Loan.Rules
{
    public interface ILoanRule
    {
        bool IsSatisfiedBy(LoanApplicationRequestDto request, LoanApplication? existingApplication);
        string ErrorMessage { get; }
    }

    public class MaxReapplicationRule : ILoanRule
    {
        public string ErrorMessage => "You have reached the maximum number of re-applications (1) for this loan.";

        public bool IsSatisfiedBy(LoanApplicationRequestDto request, LoanApplication? existingApplication)
        {
            if (existingApplication != null && existingApplication.RejectionCount >= 2)
            {
                return false;
            }
            return true;
        }
    }

    public class ValidLoanAmountRule : ILoanRule
    {
        public string ErrorMessage => "Loan amount must be greater than zero.";

        public bool IsSatisfiedBy(LoanApplicationRequestDto request, LoanApplication? existingApplication)
        {
            return ApexWorld_Backend.Common.Constants.MonetaryConstants.IsGreaterThanOrEqual(request.LoanAmount, 0.01m);
        }
    }
}
