using System.Linq;
using ApexWorld_Backend.Features.Property.DTOs;

namespace ApexWorld_Backend.Features.Property.Rules
{
    public interface IPropertyRule
    {
        bool IsSatisfiedBy(PropertyCreateDto request);
        string ErrorMessage { get; }
    }

    public class ValidCategoryRule : IPropertyRule
    {
        public string ErrorMessage => "Invalid property category. Allowed: Apartment, Villa, Plot, Commercial Buildings.";

        public bool IsSatisfiedBy(PropertyCreateDto request)
        {
            var allowed = new[] { "Apartment", "Villa", "Plot", "Commercial Buildings" };
            return allowed.Contains(request.Category);
        }
    }

    public class ValidPriceRule : IPropertyRule
    {
        public string ErrorMessage => "Price must be greater than zero.";

        public bool IsSatisfiedBy(PropertyCreateDto request)
        {
            return ApexWorld_Backend.Common.Constants.MonetaryConstants.IsGreaterThanOrEqual(request.Price, 0.01m);
        }
    }
}
