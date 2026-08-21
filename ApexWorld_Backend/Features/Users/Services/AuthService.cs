using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Users.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApexWorld_Backend.Features.Users.Services{
    public class AuthService : ApexWorld_Backend.Features.Users.Services.IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<RefreshToken> _refreshTokenRepo;
        private readonly IRepository<RevokedToken> _revokedTokenRepo;
        private readonly JwtSettings _jwtSettings;
        private readonly IAuditService _auditService;
        private readonly ApexWorld_Backend.Core.Interfaces.IEmailService _emailService;

        public AuthService(
            IRepository<User> userRepo, 
            IRepository<RefreshToken> refreshTokenRepo,
            IRepository<RevokedToken> revokedTokenRepo,
            IOptions<JwtSettings> jwtSettings, 
            IAuditService auditService,
            ApexWorld_Backend.Core.Interfaces.IEmailService emailService)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _revokedTokenRepo = revokedTokenRepo;
            _jwtSettings = jwtSettings.Value;
            _auditService = auditService;
            _emailService = emailService;
        }

        public async Task<User> RegisterBuyerAsync(RegisterBuyerDto request)
        {
            var existingUsers = await _userRepo.GetAllAsync();
            if (existingUsers.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Username is already taken.");
            }

            var buyer = new Buyer
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                City = request.City
            };

            await _userRepo.AddAsync(buyer);
            await _auditService.LogAsync("Register", "Buyer", buyer.Id.ToString(), "Registered new buyer: " + buyer.Username, buyer.Id.ToString());
            
            return buyer;
        }
        
        public async Task<User> RegisterAdminAsync(RegisterAdminDto request)
        {
            var existingUsers = await _userRepo.GetAllAsync();
            if (existingUsers.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Username is already taken.");
            }

            var admin = new Admin
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Department = request.Department,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                City = request.City
            };

            await _userRepo.AddAsync(admin);
            await _auditService.LogAsync("Register", "SubAdmin", admin.Id.ToString(), "Registered new subadmin: " + admin.Username, admin.Id.ToString());
            
            return admin;
        }

        public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request)
        {
            var existingUsers = await _userRepo.GetAsync(u => u.Username == request.Username, "UserRoles.Role");
            var user = existingUsers.FirstOrDefault();

            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                throw new Exception($"Account is temporarily locked due to multiple failed login attempts. Please try again after {user.LockoutEnd.Value.ToLocalTime():t} or reset your password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    await _userRepo.UpdateAsync(user);
                    throw new Exception("Account is temporarily locked due to multiple failed login attempts. Please try again after 15 minutes or reset your password.");
                }
                
                await _userRepo.UpdateAsync(user);
                throw new Exception($"Invalid credentials. You have {3 - user.FailedLoginAttempts} attempt(s) remaining.");
            }

            // Successful login, reset lockout stats
            if (user.FailedLoginAttempts > 0 || user.LockoutEnd.HasValue)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _userRepo.UpdateAsync(user);
            }

            if (!user.IsActive)
            {
                throw new Exception("Your account has been deactivated. Please contact support.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? "super-secret-key-change-this-in-production-must-be-32-bytes");
            
            var roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? (user is Admin ? ApexWorld.Core.Common.Roles.Admin : ApexWorld.Core.Common.Roles.Buyer);

            var claims = new System.Collections.Generic.List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes > 0 ? _jwtSettings.ExpiryMinutes : 60),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            await _refreshTokenRepo.AddAsync(refreshToken);

            await _auditService.LogAsync("Login", "User", user.Id.ToString(), "User logged in", user.Id.ToString());

            return new TokenResponseDto
            {
                AccessToken = jwtToken,
                RefreshToken = refreshTokenString
            };
        }

        public async Task<TokenResponseDto> GoogleLoginAsync(string idToken)
        {
            Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
                {
                    IssuedAtClockTolerance = TimeSpan.FromDays(3650),
                    ExpirationTimeClockTolerance = TimeSpan.FromDays(3650)
                };
                payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Google token: " + ex.Message);
            }

            // Find existing user by email
            var existingUsers = await _userRepo.GetAsync(u => u.Email == payload.Email, "UserRoles.Role");
            var user = existingUsers.FirstOrDefault();

            if (user == null)
            {
                // Auto-register a new Buyer if they don't exist
                user = new Buyer
                {
                    Username = payload.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // random unguessable password
                    FullName = payload.Name,
                    Email = payload.Email,
                    IsActive = true,
                };
                await _userRepo.AddAsync(user);
                await _auditService.LogAsync("Register", "Buyer", user.Id.ToString(), "Auto-registered Google user: " + user.Email, user.Id.ToString());
            }
            else
            {
                if (!user.IsActive)
                {
                    throw new Exception("Your account has been deactivated. Please contact support.");
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? "super-secret-key-change-this-in-production-must-be-32-bytes");
            
            var roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? (user is Admin ? ApexWorld.Core.Common.Roles.Admin : ApexWorld.Core.Common.Roles.Buyer);

            var claims = new System.Collections.Generic.List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes > 0 ? _jwtSettings.ExpiryMinutes : 60),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            await _refreshTokenRepo.AddAsync(refreshToken);

            await _auditService.LogAsync("Login", "User", user.Id.ToString(), "User logged in via Google", user.Id.ToString());

            return new TokenResponseDto
            {
                AccessToken = jwtToken,
                RefreshToken = refreshTokenString
            };
        }
        
        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? "super-secret-key-change-this-in-production-must-be-32-bytes")),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(request.AccessToken, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new Exception("Invalid access token");
            }

            var revokedTokens = await _revokedTokenRepo.GetAllAsync();
            if (revokedTokens.Any(rt => rt.Token == request.RefreshToken))
            {
                throw new Exception("This refresh token has been revoked.");
            }

            var refreshTokens = await _refreshTokenRepo.GetAllAsync();
            var storedRefreshToken = refreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken && rt.UserId == userId);
            
            if (storedRefreshToken == null || storedRefreshToken.ExpiryTime < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired refresh token");
            }
            
            var users = await _userRepo.GetAsync(u => u.Id == userId, "UserRoles.Role");
            var user = users.FirstOrDefault();
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var newKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? "super-secret-key-change-this-in-production-must-be-32-bytes");

            // Re-include the role claim so role-restricted endpoints continue to work after token refresh
            var roleName = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? (user is Admin ? ApexWorld.Core.Common.Roles.Admin : ApexWorld.Core.Common.Roles.Buyer);

            var claims = new System.Collections.Generic.List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName)
            };

            var newTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes > 0 ? _jwtSettings.ExpiryMinutes : 60),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(newKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var newToken = tokenHandler.CreateToken(newTokenDescriptor);
            var newJwtToken = tokenHandler.WriteToken(newToken);
            
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var newRefreshTokenString = Convert.ToBase64String(randomNumber);

            storedRefreshToken.Token = newRefreshTokenString;
            storedRefreshToken.ExpiryTime = DateTime.UtcNow.AddDays(7);
            await _refreshTokenRepo.UpdateAsync(storedRefreshToken);

            return new TokenResponseDto
            {
                AccessToken = newJwtToken,
                RefreshToken = newRefreshTokenString
            };
        }
        
        public async Task LogoutAsync(string userId, string? refreshToken = null)
        {
            if (int.TryParse(userId, out int id))
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user != null)
                {
                    var refreshTokens = await _refreshTokenRepo.GetAllAsync();
                    var userTokens = refreshTokens.Where(rt => rt.UserId == id).ToList();

                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        userTokens = userTokens.Where(rt => rt.Token == refreshToken).ToList();
                        
                        if (!userTokens.Any())
                        {
                            var revoked = new RevokedToken { Token = refreshToken, RevokedAt = DateTime.UtcNow };
                            await _revokedTokenRepo.AddAsync(revoked);
                        }
                    }

                    foreach(var rt in userTokens)
                    {
                        var revoked = new RevokedToken { Token = rt.Token, RevokedAt = DateTime.UtcNow };
                        await _revokedTokenRepo.AddAsync(revoked);
                        await _refreshTokenRepo.DeleteAsync(rt);
                    }
                    
                    await _auditService.LogAsync("Logout", "User", user.Id.ToString(), "User logged out.", user.Id.ToString());
                }
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto request)
        {
            var email = request.Email;
            var users = await _userRepo.GetAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            
            if (user != null)
            {
                var rng = new Random();
                string otp = rng.Next(100000, 999999).ToString();

                user.ResetToken = otp;
                user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                await _userRepo.UpdateAsync(user);

                var emailBody = $"Your password reset OTP is: <b>{otp}</b>. It is valid for 15 minutes.";
                await _emailService.SendEmailAsync(user.Email, "Password Reset", emailBody);

                await _auditService.LogAsync("ForgotPassword", "User", user.Id.ToString(), $"Password reset OTP sent to {user.Email}", user.Id.ToString());
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var email = request.Email;
            var users = await _userRepo.GetAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            
            if (user == null || user.ResetToken != request.Token || user.ResetTokenExpiry < DateTime.UtcNow) 
            {
                throw new Exception("Invalid or expired reset request.");
            }

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            {
                throw new Exception("The new password must be different from the current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            
            // Unlock account upon successful password reset
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            
            await _userRepo.UpdateAsync(user);
            
            await _auditService.LogAsync("ResetPassword", "User", user.Id.ToString(), $"Password reset successfully for {user.Email}", user.Id.ToString());
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto request)
        {
            if (int.TryParse(userId, out int id))
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    throw new Exception("Invalid current password.");
                }
                
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _userRepo.UpdateAsync(user);
                
                await _auditService.LogAsync("ChangePassword", "User", user.Id.ToString(), "User changed password.", user.Id.ToString());
            }
            else 
            {
                throw new Exception("Invalid user ID.");
            }
        }
    }
}
