using ApexWorld_Backend.Features.Wishlist.Exceptions;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Features.Wishlist.Rules;

namespace ApexWorld_Backend.Features.Wishlist.Validators
{
    public class WishlistRequestValidator
    {
        private readonly IEnumerable<IWishlistRule<int>> _rules;

        public WishlistRequestValidator(IEnumerable<IWishlistRule<int>> rules)
        {
            _rules = rules;
        }

        public void ValidatePropertyId(int propertyId)
        {
            var errors = new List<string>();
            foreach (var rule in _rules)
            {
                var error = rule.Validate(propertyId);
                if (error != null) errors.Add(error);
            }
            if (errors.Count > 0) throw new WishlistValidationException(errors);
        }
    }
}




