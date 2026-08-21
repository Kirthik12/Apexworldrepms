namespace ApexWorld_Backend.Features.Booking.DTOs{
    public class BookingRequestDto
    {
        public int PropertyId { get; set; }
        public int BuyerId { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PermanentAddress { get; set; }
    }
}
