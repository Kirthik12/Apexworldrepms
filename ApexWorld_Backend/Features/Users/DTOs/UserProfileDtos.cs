using System;
using System.ComponentModel.DataAnnotations;

namespace ApexWorld_Backend.Features.Users.DTOs{
    public class BuyerProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? BuyerAccountId { get; set; }
        public string? PanCardKycStatus { get; set; }
        public int? CreditScore { get; set; }
        public string? AccountStatus { get; set; }
        public DateTime MemberSince { get; set; }
    }

    public class UpdateBuyerProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
    }

    public class AdminProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateAdminProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}

