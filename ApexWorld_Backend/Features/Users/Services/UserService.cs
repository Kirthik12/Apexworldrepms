using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Users.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings

namespace ApexWorld_Backend.Features.Users.Services{
    public class UserService : ApexWorld_Backend.Features.Users.Services.IUserService
    {
        private readonly IRepository<User> _userRepo;

        public UserService(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<BuyerProfileDto?> GetBuyerProfileAsync(int userId)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return null;

            return new BuyerProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.City,
//                 BuyerAccountId = user.BuyerAccountId ?? $"BUYER-{user.Id:0000}", // Auto-generate if missing
//                 PanCardKycStatus = user.PanCardKycStatus ?? "Pending",
//                 CreditScore = user.CreditScore ?? 0,
//                 AccountStatus = user.AccountStatus ?? "Active",
                MemberSince = user.CreatedAt
            };
        }

        public async Task<bool> UpdateBuyerProfileAsync(int userId, UpdateBuyerProfileDto dto)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.City = dto.City;

            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteBuyerAccountAsync(int userId)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return false;

            // Soft delete
            user.IsDeleted = true;
            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<AdminProfileDto?> GetAdminProfileAsync(int userId)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return null;

            return new AdminProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? string.Empty
            };
        }

        public async Task<bool> UpdateAdminProfileAsync(int userId, UpdateAdminProfileDto dto)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            // user.Role = dto.Role; // Updating role through this endpoint is no longer supported directly via User entity

            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteSubAdminAsync(int userId)
        {
            var users = await _userRepo.GetAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            if (user == null) return false;

            // Hard delete for now, or soft delete
            await _userRepo.DeleteAsync(user);
            return true;
        }
    }
}





