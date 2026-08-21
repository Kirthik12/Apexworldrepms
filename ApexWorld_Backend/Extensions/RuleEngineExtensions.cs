using Microsoft.Extensions.DependencyInjection;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Services;
using System.Linq;
using System.Reflection;

namespace ApexWorld_Backend.Extensions
{
    public static class RuleEngineExtensions
    {
        public static IServiceCollection AddRuleEngine(this IServiceCollection services)
        {
            // Register the generic engine
            services.AddScoped(typeof(IRuleEngine<>), typeof(RuleEngine<>));

            // Automatically scan and register all concrete IRule<T> implementations
            var assembly = Assembly.GetExecutingAssembly();
            
            var ruleTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (type, implementedInterface) => new { type, implementedInterface })
                .Where(x => x.implementedInterface.IsGenericType && 
                            x.implementedInterface.GetGenericTypeDefinition() == typeof(IRule<>));

            foreach (var rule in ruleTypes)
            {
                services.AddScoped(rule.implementedInterface, rule.type);
            }

            return services;
        }
    }
}
