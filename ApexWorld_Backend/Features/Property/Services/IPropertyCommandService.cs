using ApexWorld_Backend.Features.Property.DTOs;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Property.Services{
    public interface IPropertyCommandService
    {
        Task<ApexWorld_Backend.Features.Property.Models.Property> AddPropertyAsync(PropertyCreateDto request);
        Task<ApexWorld_Backend.Features.Property.Models.Property> UpdatePropertyAsync(int id, PropertyUpdateDto request);
        Task<ApexWorld_Backend.Features.Property.Models.Property> UpdatePropertyStatusAsync(int id, PropertyStatusUpdateDto request);
        Task DeletePropertyAsync(int id);
    }
}




