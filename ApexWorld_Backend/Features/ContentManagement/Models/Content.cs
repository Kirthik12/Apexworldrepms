using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.ContentManagement.Models
{
    public class Content : BaseEntity
    {
        public string Section { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text";
        public bool IsActive { get; set; } = true;
    }
}
