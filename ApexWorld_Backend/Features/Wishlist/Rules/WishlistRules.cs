namespace ApexWorld_Backend.Features.Wishlist.Rules
{
    public interface IWishlistRule<T>
    {
        string? Validate(T request);
    }

    public class ValidPropertyIdRule : IWishlistRule<int>
    {
        public string? Validate(int propertyId)
        {
            if (propertyId <= 0)
            {
                return "Property ID must be greater than zero.";
            }
            return null;
        }
    }
}

