using ApexWorld_Backend.Features.Loan.Exceptions;
using ApexWorld_Backend.Features.Loan.DTOs;
using ApexWorld_Backend.Features.Loan.Services;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Loan.Controllers
{
    [Tags("Buyer - Loans")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ICurrentUserService _currentUserService;

        public LoanController(ILoanService loanService, ICurrentUserService currentUserService)
        {
            _loanService = loanService;
            _currentUserService = currentUserService;
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationRequestDto request)
        {
            try
            {
                // Force the BuyerId to the logged-in user
                request.BuyerId = int.Parse(_currentUserService.UserId!);
                var application = await _loanService.ApplyForLoanAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(application));
            }
            catch (MaxReapplicationReachedException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("my-loans")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> GetMyLoans()
        {
            var buyerId = int.Parse(_currentUserService.UserId!);
            var loans = await _loanService.GetBuyerLoansAsync(buyerId);
            return Ok(ApiResponse<object>.SuccessResponse(loans));
        }
    }

    [Tags("Admin - Loans")]
    [ApiController]
    [Route("api/v1/admin/Loan")]
    public class AdminLoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public AdminLoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(ApiResponse<object>.SuccessResponse(loans));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] LoanStatusUpdateDto request)
        {
            try
            {
                var loan = await _loanService.UpdateLoanStatusAsync(id, request.Status);
                return Ok(ApiResponse<object>.SuccessResponse(loan));
            }
            catch (LoanApplicationNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
