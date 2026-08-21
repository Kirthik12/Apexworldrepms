namespace ApexWorld_Backend.Features.Users.Models{
    public class Admin : User
    {
        // Admin specific fields can go here
        public string? Department { get; set; }
    }
}
