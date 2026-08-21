namespace ApexWorld_Backend.Common.Interfaces
{
    public interface IRule<in T>
    {
        bool IsSatisfiedBy(T context);
        string ErrorMessage { get; }
    }
}
