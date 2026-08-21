using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Users.Controllers
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; } = "Buyer";
    }

    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApexWorld_Backend.Data.ApplicationDbContext _dbContext;
        private readonly ApexWorld_Backend.Common.Interfaces.ICurrentUserService _currentUserService;

        public UsersController(ApexWorld_Backend.Data.ApplicationDbContext dbContext, ApexWorld_Backend.Common.Interfaces.ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async System.Threading.Tasks.Task<IActionResult> GetMyProfile()
        {
            var userIdStr = _currentUserService.UserId;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_dbContext.Users, u => u.Id == userId);
            if (user == null) return NotFound();

            var nameParts = string.IsNullOrEmpty(user.FullName) ? new string[0] : user.FullName.Split(' ');
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", System.Linq.Enumerable.Skip(nameParts, 1)) : "";

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.City,
                Role = "Buyer"
            };

            return Ok(dto);
        }

        [HttpPut("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async System.Threading.Tasks.Task<IActionResult> UpdateMyProfile([FromBody] UserProfileDto dto)
        {
            var userIdStr = _currentUserService.UserId;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_dbContext.Users, u => u.Id == userId);
            if (user == null) return NotFound();

            user.FullName = (dto.FirstName + " " + (dto.LastName ?? "")).Trim();
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.City = dto.Address;

            await _dbContext.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet("buyers")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> GetBuyers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = _dbContext.Buyers.AsQueryable();

            var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);

            var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                query.OrderByDescending(u => u.CreatedAt)
                     .Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize)
            );

            return Ok(new { Items = items, TotalCount = totalCount });
        }

        [HttpPatch("{id}/toggle-active")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async System.Threading.Tasks.Task<IActionResult> ToggleUserActiveStatus(int id)
        {
            var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_dbContext.Users, u => u.Id == id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User active status updated", isActive = user.IsActive });
        }
    }
}
