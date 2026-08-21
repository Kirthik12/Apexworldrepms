using ApexWorld_Backend.Features.Enquiry.Exceptions;
using ApexWorld_Backend.Features.Enquiry.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Enquiry.Validators;
using EnquiryEntity = ApexWorld_Backend.Features.Enquiry.Models.Enquiry;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Enquiry.Services{
    public class EnquiryService : ApexWorld_Backend.Features.Enquiry.Services.IEnquiryService
    {
        private readonly IRepository<EnquiryEntity> _enquiryRepo;
        private readonly IAdminNotificationService _adminNotificationService;

        public EnquiryService(IRepository<EnquiryEntity> enquiryRepo, IAdminNotificationService adminNotificationService)
        {
            _enquiryRepo = enquiryRepo;
            _adminNotificationService = adminNotificationService;
        }

        public async Task<EnquiryEntity> SubmitEnquiryAsync(EnquiryRequestDto request)
        {
            var validator = new EnquiryRequestValidator();
            var (isValid, errors) = validator.Validate(request);
            if (!isValid)
            {
                throw new InvalidEnquiryException(string.Join(", ", errors));
            }

            var enquiry = new EnquiryEntity
            {
                BuyerName = request.BuyerName,
                Phone = request.Phone,
                Email = request.Email,
                Message = request.Message,
                Status = "New"
            };

            try
            {
                await _enquiryRepo.AddAsync(enquiry);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                throw new Exception("DB Error: " + (ex.InnerException?.Message ?? ex.Message));
            }

            await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
            {
                Title = "New Enquiry Received",
                Message = $"New enquiry received from {request.BuyerName}: '{request.Message.Substring(0, System.Math.Min(request.Message.Length, 50))}...'",
                Category = "Enquiries",
                TargetAudience = "SpecificRole",
                TargetRole = "Admin"
            }, 0);

            return enquiry;
        }

        public async Task<List<EnquiryEntity>> GetAdminEnquiriesAsync()
        {
            return (await _enquiryRepo.GetAllAsync()).ToList();
        }

        public async Task ResolveEnquiryAsync(int enquiryId, string adminResponse)
        {
            var enquiry = await _enquiryRepo.GetByIdAsync(enquiryId);
            if (enquiry == null)
            {
                throw new InvalidEnquiryException($"Enquiry with ID {enquiryId} not found.");
            }

            enquiry.Status = "Resolved";
            enquiry.AdminResponse = adminResponse;
            enquiry.ResponseDate = System.DateTime.UtcNow;
            await _enquiryRepo.UpdateAsync(enquiry);
        }

        public async Task DeleteEnquiryAsync(int enquiryId)
        {
            var enquiry = await _enquiryRepo.GetByIdAsync(enquiryId);
            if (enquiry != null)
            {
                await _enquiryRepo.DeleteAsync(enquiry);
            }
        }
    }
}







