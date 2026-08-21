using ApexWorld_Backend.Features.Enquiry.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Enquiry.Services
{
    public interface IEnquiryService
    {
        Task<ApexWorld_Backend.Features.Enquiry.Models.Enquiry> SubmitEnquiryAsync(EnquiryRequestDto request);
        Task<List<ApexWorld_Backend.Features.Enquiry.Models.Enquiry>> GetAdminEnquiriesAsync();
        Task ResolveEnquiryAsync(int enquiryId, string adminResponse);
        Task DeleteEnquiryAsync(int enquiryId);
    }
}
