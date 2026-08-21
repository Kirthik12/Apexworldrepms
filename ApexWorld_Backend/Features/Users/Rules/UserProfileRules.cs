using System.Text.RegularExpressions;
using ApexWorld_Backend.Features.Users.DTOs;

namespace ApexWorld_Backend.Features.Users.Rules
{
    public interface IUserProfileRule<T>
    {
        string? Validate(T request);
    }

    public class BuyerEmailFormatRule : IUserProfileRule<UpdateBuyerProfileDto>
    {
        public string? Validate(UpdateBuyerProfileDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return null;

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!emailRegex.IsMatch(request.Email))
            {
                return "Invalid email format.";
            }
            return null;
        }
    }

    public class BuyerPhoneNumberFormatRule : IUserProfileRule<UpdateBuyerProfileDto>
    {
        public string? Validate(UpdateBuyerProfileDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber)) return null;

            var phoneRegex = new Regex(@"^[6-9]\d{9}$", RegexOptions.Compiled);
            if (!phoneRegex.IsMatch(request.PhoneNumber))
            {
                return "Invalid phone number format. It must contain 10 digits and start with 6, 7, 8, or 9.";
            }
            return null;
        }
    }

    public class AdminRoleRule : IUserProfileRule<UpdateAdminProfileDto>
    {
        public string? Validate(UpdateAdminProfileDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return "Role is required for Admins.";
            }
            return null;
        }
    }
}

