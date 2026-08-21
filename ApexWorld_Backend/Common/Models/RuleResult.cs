using System.Collections.Generic;
using System.Linq;

namespace ApexWorld_Backend.Common.Models
{
    public class RuleResult
    {
        public bool IsSuccess => !Errors.Any();
        public List<string> Errors { get; } = new List<string>();

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }
    }
}
