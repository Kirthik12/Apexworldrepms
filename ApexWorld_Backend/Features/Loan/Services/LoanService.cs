using ApexWorld_Backend.Features.Loan.Exceptions;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Loan.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Loan.Validators;
using ApexWorld_Backend.Features.Payment.Models;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Loan.Services{
    public class LoanService : ApexWorld_Backend.Features.Loan.Services.ILoanService
    {
        private readonly IRepository<LoanApplication> _loanRepo;
        private readonly IRepository<PaymentRecord> _paymentRepo;
        private readonly IRepository<BookingEntity> _bookingRepo;
        private readonly ApexWorld_Backend.Features.Booking.Services.IBookingService _bookingService;
        private readonly ApexWorld_Backend.Common.Services.IBulkheadService _bulkheadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<EMIPlan> _emiPlanRepo;
        private readonly IRepository<ApexWorld_Backend.Features.Property.Models.Property> _propertyRepo;
        private readonly IAdminNotificationService _adminNotificationService;
        private readonly IBuyerNotificationService _buyerNotificationService;

        public LoanService(
            IRepository<LoanApplication> loanRepo, 
            IRepository<PaymentRecord> paymentRepo, 
            IRepository<BookingEntity> bookingRepo, 
            ApexWorld_Backend.Features.Booking.Services.IBookingService bookingService, 
            ApexWorld_Backend.Common.Services.IBulkheadService bulkheadService,
            IUnitOfWork unitOfWork,
            IRepository<EMIPlan> emiPlanRepo,
            IRepository<ApexWorld_Backend.Features.Property.Models.Property> propertyRepo,
            IAdminNotificationService adminNotificationService,
            IBuyerNotificationService buyerNotificationService)
        {
            _loanRepo = loanRepo;
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _bookingService = bookingService;
            _bulkheadService = bulkheadService;
            _unitOfWork = unitOfWork;
            _emiPlanRepo = emiPlanRepo;
            _propertyRepo = propertyRepo;
            _adminNotificationService = adminNotificationService;
            _buyerNotificationService = buyerNotificationService;
        }

        public async Task<LoanApplication> ApplyForLoanAsync(LoanApplicationRequestDto request)
        {
            return await _bulkheadService.ExecuteAsync("Loan", async () =>
            {
                var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    throw new System.Exception("Booking not found.");
                }
                
                request.PropertyId = booking.PropertyId;
                request.BuyerId = booking.BuyerId;
                request.BuyerName = $"{booking.FirstName} {booking.LastName}";
                if (string.IsNullOrEmpty(request.BankName))
                {
                    request.BankName = "SBI Home Loans";
                }

                // Check if there is an existing payment for this booking
                var existingPayment = (await _paymentRepo.GetAsync(p => p.BookingId == request.BookingId && (p.Status == "Pending" || p.Status == "Success"))).FirstOrDefault();
                if (existingPayment != null)
                {
                    throw new System.Exception("A payment has already been initiated or completed for this booking. Cannot apply for a loan.");
                }

                // Check if there's already an application for this buyer and property
                var existingApplication = (await _loanRepo.GetAsync(l => l.BuyerId == request.BuyerId && l.PropertyId == request.PropertyId)).FirstOrDefault();

                var validator = new LoanRequestValidator();
                var (isValid, errors) = validator.Validate(request, existingApplication);

                if (!isValid)
                {
                    if (errors.Any(e => e.Contains("maximum number of re-applications")))
                    {
                        throw new MaxReapplicationReachedException();
                    }
                    throw new System.Exception(string.Join(", ", errors));
                }

                decimal monthlyInterestRate = (8.5m / 100m) / 12m;
                int months = request.TenureYears * 12;
                double r = (double)monthlyInterestRate;
                double n = (double)months;
                double p = (double)request.LoanAmount;
                decimal monthlyEmi = (months > 0) ? (decimal)(p * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1)) : 0m;

                // Lock booking and property
                booking.Status = "Waiting for Bank Approval";
                await _bookingRepo.UpdateAsync(booking);

                var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                if (property != null)
                {
                    property.IsAvailable = false;
                    await _propertyRepo.UpdateAsync(property);
                }

                if (existingApplication != null)
                {
                    // Re-apply for the loan
                    if (existingApplication.Status == "Rejected")
                    {
                        existingApplication.RejectionCount += 1;
                    }
                    existingApplication.Status = "Pending";
                    existingApplication.LoanAmount = request.LoanAmount;
                    existingApplication.BankName = request.BankName;
                    existingApplication.BookingId = request.BookingId;
                    existingApplication.BuyerName = request.BuyerName;
                    existingApplication.TenureYears = request.TenureYears;
                    existingApplication.EmploymentType = request.EmploymentType;
                    existingApplication.MonthlyIncome = request.MonthlyIncome;
                    existingApplication.MonthlyEMI = monthlyEmi;
                    
                    await _loanRepo.UpdateAsync(existingApplication);
                    
                    await SendLoanApplicationNotifications(existingApplication.BuyerId, existingApplication.PropertyId, existingApplication.Id);
                    return existingApplication;
                }

                // New application
                var newApplication = new LoanApplication
                {
                    BuyerId = request.BuyerId,
                    BuyerName = request.BuyerName,
                    BookingId = request.BookingId,
                    PropertyId = request.PropertyId,
                    LoanAmount = request.LoanAmount,
                    BankName = request.BankName,
                    TenureYears = request.TenureYears,
                    EmploymentType = request.EmploymentType,
                    MonthlyIncome = request.MonthlyIncome,
                    MonthlyEMI = monthlyEmi,
                    Status = "Pending",
                    RejectionCount = 0
                };

                await _loanRepo.AddAsync(newApplication);
                
                await SendLoanApplicationNotifications(newApplication.BuyerId, newApplication.PropertyId, newApplication.Id);
                return newApplication;
            });
        }
        
        private async Task SendLoanApplicationNotifications(int buyerId, int propertyId, int loanId)
        {
            await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
            {
                Title = "New Loan Application",
                Message = $"New loan application received from Buyer {buyerId} for Property {propertyId}.",
                Category = "Loans",
                TargetAudience = "SpecificRole",
                TargetRole = "Admin"
            }, 0);

            await _buyerNotificationService.CreateBuyerNotificationAsync(new CreateBuyerNotificationDto
            {
                BuyerId = buyerId,
                Title = "Loan Application Submitted",
                Message = $"Your loan application for property {propertyId} has been successfully submitted and is under review.",
                Category = "Loans",
                ActionText = "View Loan",
                ActionUrl = $"/api/v1/Loans/{loanId}",
                RelatedEntityType = "Loan",
                RelatedEntityId = loanId
            });
        }

        public async Task<List<LoanApplication>> GetBuyerLoansAsync(int buyerId)
        {
            return (await _loanRepo.GetAsync(l => l.BuyerId == buyerId, "Property,Booking")).ToList();
        }

        public async Task<List<LoanApplication>> GetAllLoansAsync()
        {
            return (await _loanRepo.GetAsync(l => true, "Property,Booking")).ToList();
        }

        public async Task<LoanApplication> UpdateLoanStatusAsync(int loanId, string newStatus)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null)
            {
                throw new LoanApplicationNotFoundException(loanId);
            }

            if (loan.Status != "Pending")
            {
                throw new System.Exception($"Cannot update loan application status because it is already {loan.Status}.");
            }

            var property = await _propertyRepo.GetByIdAsync(loan.PropertyId);
            if (property == null)
            {
                throw new System.Exception("The property associated with this loan application no longer exists.");
            }

            loan.Status = newStatus; // "Approved" or "Rejected"
            await _loanRepo.UpdateAsync(loan);

            if (newStatus == "Approved")
            {
                if (property.Status == "Booked")
                {
                    throw new System.Exception("This property is already booked by another buyer.");
                }

                var booking = await _bookingRepo.GetByIdAsync(loan.BookingId);
                if (booking != null)
                {
                    booking.Status = "Booked";
                    await _bookingRepo.UpdateAsync(booking);
                    
                    property.Status = "Booked";
                    property.IsAvailable = false;
                    await _propertyRepo.UpdateAsync(property);

                    // Generate real EMI plans
                    await GenerateEMIPlansAsync(loan.Id, loan.TenureYears * 12, loan.LoanAmount, 8.5m);
                }
            }
            else if (newStatus == "Rejected")
            {
                await _bookingService.CancelBookingDueToLoanRejectionAsync(loan.BookingId);
            }

            await _buyerNotificationService.CreateBuyerNotificationAsync(new CreateBuyerNotificationDto
            {
                BuyerId = loan.BuyerId,
                Title = "Loan Application Updated",
                Message = $"Your loan application status has been updated to: {newStatus}.",
                Category = "Loans",
                ActionText = "View Loan",
                ActionUrl = $"/api/v1/Loans/{loan.Id}",
                RelatedEntityType = "Loan",
                RelatedEntityId = loan.Id
            });

            return loan;
        }

        public async Task GenerateEMIPlansAsync(int loanApplicationId, int durationMonths, decimal totalAmount, decimal annualInterestRate)
        {
            // Calculate base monthly interest rate
            decimal monthlyInterestRate = (annualInterestRate / 100) / 12;
            
            // Transactional chunking for large loops
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                int batchSize = 100;
                
                for (int i = 1; i <= durationMonths; i++)
                {
                    // Calculate EMI using standard formula: P x R x (1+R)^N / [(1+R)^N-1]
                    // (Simplified calculation for demonstration purposes)
                    decimal installmentAmount = totalAmount / durationMonths + (totalAmount * monthlyInterestRate);
                    
                    var plan = new EMIPlan
                    {
                        LoanApplicationId = loanApplicationId,
                        InstallmentAmount = installmentAmount,
                        Months = i, // representing the month number in this context
                        InterestRate = annualInterestRate
                    };
                    
                    await _emiPlanRepo.AddAsync(plan);

                    // Batch save every 100 records to prevent memory exhaustion and long-running transaction locks
                    if (i % batchSize == 0)
                    {
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                
                // Final save for any remaining records
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
