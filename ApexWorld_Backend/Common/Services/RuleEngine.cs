using System.Collections.Generic;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Models;

namespace ApexWorld_Backend.Common.Services
{
    public class RuleEngine<T> : IRuleEngine<T>
    {
        private readonly IEnumerable<IRule<T>> _rules;

        public RuleEngine(IEnumerable<IRule<T>> rules)
        {
            _rules = rules;
        }

        public RuleResult Evaluate(T context)
        {
            var result = new RuleResult();
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(context))
                {
                    result.AddError(rule.ErrorMessage);
                }
            }
            return result;
        }
    }
}
