using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Property.Models{
    public class PropertyImage : BaseEntity
    {
        public int PropertyId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        
        public Property? Property { get; set; }
    }
}
