using ApexWorld_Backend.Common.Models;

namespace ApexWorld_Backend.Common.Interfaces
{
    public interface IRuleEngine<in T>
    {
        RuleResult Evaluate(T context);
    }
}
