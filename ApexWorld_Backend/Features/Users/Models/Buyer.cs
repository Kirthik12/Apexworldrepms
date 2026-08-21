namespace ApexWorld_Backend.Features.Users.Models{
    public class Buyer : User
    {
        public string? BuyerAccountId { get; set; }
        public string? PanCardKycStatus { get; set; }
        public int? CreditScore { get; set; }
        public string? AccountStatus { get; set; }
    }
}
