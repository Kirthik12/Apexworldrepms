using ApexWorld_Backend.Features.Property.Services;
using ApexWorld_Backend.Features.Property.DTOs;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Http;

namespace ApexWorld_Backend.Modules.Property.Controllers
{
    [Tags("Buyer - Properties")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyQueryService _propertyService;
        private readonly IRepository<ApexWorld_Backend.Features.Property.Models.Property> _propertyRepo;

        public PropertyController(IPropertyQueryService propertyService, IRepository<ApexWorld_Backend.Features.Property.Models.Property> propertyRepo)
        {
            _propertyService = propertyService;
            _propertyRepo = propertyRepo;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Buyer)]
        [OutputCache(PolicyName = "PropertyCache")]
        public async Task<IActionResult> GetListedProperties([FromQuery] string? category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var properties = await _propertyService.GetListedPropertiesAsync(category);
            var results = properties.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var response = new { TotalItems = properties.Count, PageNumber = pageNumber, PageSize = pageSize, Items = results };
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Buyer)]
        [OutputCache(PolicyName = "PropertyCache")]
        public async Task<IActionResult> GetPropertyDetails(int id)
        {
            try
            {
                var property = await _propertyService.GetPropertyDetailsAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(property));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = Roles.Buyer)]
        [OutputCache(PolicyName = "PropertyCache")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Fixed")]
        public async Task<IActionResult> SearchProperties([FromQuery] string? status = "Available", [FromQuery] string? name = null, [FromQuery] int? id = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var allProperties = await _propertyRepo.GetAllAsync();
            var query = allProperties.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (id.HasValue)
            {
                query = query.Where(p => p.Id == id.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Title.Contains(name, StringComparison.OrdinalIgnoreCase) || p.ProjectName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = query.Count();
            var results = query.OrderByDescending(p => p.CreatedAt)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToList();

            var response = new 
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = results
            };

            return Ok(ApiResponse<object>.SuccessResponse(response));
        }
    }

    [Tags("Admin - Properties")]
    [ApiController]
    [Route("api/v1/admin/[controller]")]
    public class AdminPropertyController : ControllerBase
    {
        private readonly IPropertyCommandService _propertyService;
        private readonly IOutputCacheStore _cacheStore;
        private readonly IRepository<ApexWorld_Backend.Features.Property.Models.Property> _propertyRepo;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public AdminPropertyController(IPropertyCommandService propertyService, IOutputCacheStore cacheStore, IRepository<ApexWorld_Backend.Features.Property.Models.Property> propertyRepo, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _propertyService = propertyService;
            _cacheStore = cacheStore;
            _propertyRepo = propertyRepo;
            _env = env;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllProperties([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var properties = await _propertyRepo.GetAsync(p => true, "Category,Images");
            var results = properties.OrderByDescending(p => p.CreatedAt)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var response = new { TotalItems = properties.Count(), PageNumber = pageNumber, PageSize = pageSize, Items = results };
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Property not found"));
                
            return Ok(ApiResponse<object>.SuccessResponse(property));
        }



        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AddProperty([FromForm] PropertyCreateDto request)
        {
            try
            {
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    var webRoot = string.IsNullOrEmpty(_env.WebRootPath) ? Path.Combine(_env.ContentRootPath, "wwwroot") : _env.WebRootPath;
                    var uploadsFolder = Path.Combine(webRoot, "images", "properties");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ImageFile.CopyToAsync(fileStream);
                    }

                    // Add the URL to the DTO before sending to service
                    var requestUrl = $"{Request.Scheme}://{Request.Host}";
                    request.ImageUrls = new System.Collections.Generic.List<string> { $"{requestUrl}/images/properties/{uniqueFileName}" };
                }

                var property = await _propertyService.AddPropertyAsync(request);
                await _cacheStore.EvictByTagAsync("properties", default);
                return Ok(ApiResponse<object>.SuccessResponse(property));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"Failed to add property: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyUpdateDto request)
        {
            try
            {
                var property = await _propertyService.UpdatePropertyAsync(id, request);
                await _cacheStore.EvictByTagAsync("properties", default);
                return Ok(ApiResponse<object>.SuccessResponse(property));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdatePropertyStatus(int id, [FromBody] PropertyStatusUpdateDto request)
        {
            try
            {
                var property = await _propertyService.UpdatePropertyStatusAsync(id, request);
                await _cacheStore.EvictByTagAsync("properties", default);
                return Ok(ApiResponse<object>.SuccessResponse(property));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            try
            {
                await _propertyService.DeletePropertyAsync(id);
                await _cacheStore.EvictByTagAsync("properties", default);
                return Ok(ApiResponse<string>.SuccessResponse("Property deleted successfully."));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}

