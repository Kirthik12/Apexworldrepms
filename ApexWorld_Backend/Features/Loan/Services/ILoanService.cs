using ApexWorld_Backend.Features.Loan.DTOs;
using ApexWorld_Backend.Features.Loan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings

namespace ApexWorld_Backend.Features.Loan.Services{
    public interface ILoanService
    {
        Task<LoanApplication> ApplyForLoanAsync(LoanApplicationRequestDto request);
        Task<List<LoanApplication>> GetBuyerLoansAsync(int buyerId);
        Task<List<LoanApplication>> GetAllLoansAsync();
        Task<LoanApplication> UpdateLoanStatusAsync(int loanId, string newStatus);
        Task GenerateEMIPlansAsync(int loanApplicationId, int durationMonths, decimal totalAmount, decimal annualInterestRate);
    }
}



