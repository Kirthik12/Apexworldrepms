using System;

namespace ApexWorld_Backend.Common.Constants
{
    public static class MonetaryConstants
    {
        public const decimal MonetaryTolerance = 0.01m;
        
        public static bool IsEqual(decimal amount1, decimal amount2)
        {
            return Math.Abs(amount1 - amount2) <= MonetaryTolerance;
        }

        public static bool IsGreaterThanOrEqual(decimal amount1, decimal amount2)
        {
            return amount1 >= amount2 - MonetaryTolerance;
        }

        public static bool IsLessThanOrEqual(decimal amount1, decimal amount2)
        {
            return amount1 <= amount2 + MonetaryTolerance;
        }
    }
}
