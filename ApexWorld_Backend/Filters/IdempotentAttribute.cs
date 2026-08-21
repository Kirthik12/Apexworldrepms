using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApexWorld_Backend.Filters
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class IdempotentAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return (IFilterMetadata)serviceProvider.GetService(typeof(IdempotencyFilter))!;
        }
    }
}
