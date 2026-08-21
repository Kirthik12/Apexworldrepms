using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Users.DTOs;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Users.Services{
    public interface IAuthService
    {
        Task<User> RegisterBuyerAsync(RegisterBuyerDto request);
        Task<User> RegisterAdminAsync(RegisterAdminDto request);
        Task<TokenResponseDto> LoginAsync(LoginRequestDto request);
        Task<TokenResponseDto> GoogleLoginAsync(string idToken);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task LogoutAsync(string userId, string? refreshToken = null);
        Task ForgotPasswordAsync(ForgotPasswordDto request);
        Task ResetPasswordAsync(ResetPasswordDto request);
        Task ChangePasswordAsync(string userId, ChangePasswordDto request);
    }
}
