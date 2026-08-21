using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Property.Services{
    public interface IPropertyQueryService
    {
        Task<List<ApexWorld_Backend.Features.Property.Models.Property>> GetListedPropertiesAsync(string? category = null);
        Task<ApexWorld_Backend.Features.Property.Models.Property> GetPropertyDetailsAsync(int id);
        Task<System.Collections.Generic.IEnumerable<ApexWorld_Backend.Features.Property.Models.Property>> SearchPropertiesAsync(string? query, string? category, decimal? minPrice, decimal? maxPrice);
    }
}



