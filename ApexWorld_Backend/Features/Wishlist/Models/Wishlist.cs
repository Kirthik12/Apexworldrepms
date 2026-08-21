using ApexWorld.Core.Common;
using ApexWorld_Backend.Features.Users.Models;

namespace ApexWorld_Backend.Features.Wishlist.Models{
    public class Wishlist : BaseEntity
    {
        public int BuyerId { get; set; }
        public int PropertyId { get; set; }
        
        public Buyer? Buyer { get; set; }
        public ApexWorld_Backend.Features.Property.Models.Property? Property { get; set; }
    }
}
