using System;

namespace ApexWorld_Backend.Features.Loan.Exceptions
{
    public class LoanApplicationNotFoundException : Exception
    {
        public LoanApplicationNotFoundException() : base("Entity was not found.") { }
        public LoanApplicationNotFoundException(string message) : base(message) { }
        public LoanApplicationNotFoundException(int id) : base("Entity with ID " + id + " was not found.") { }
    }
}
