using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Property.DTOs;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Property.Services{
    public class PropertyService : IPropertyQueryService, IPropertyCommandService
    {
        private readonly IRepository<Models.Property> _propertyRepo;
        private readonly IReadOnlyRepository<Models.Property> _propertyReadOnlyRepo;
        private readonly IRepository<PropertyCategory> _categoryRepo;
        private readonly IRepository<PropertyImage> _imageRepo;
        private readonly IPropertyCancellationSagaService _sagaService;
        private readonly IMemoryCache _cache;
        private readonly ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService _webhookService;
        private readonly IAdminNotificationService _adminNotificationService;

        public PropertyService(
            IRepository<Models.Property> propertyRepo,
            IReadOnlyRepository<Models.Property> propertyReadOnlyRepo,
            IRepository<PropertyCategory> categoryRepo,
            IRepository<PropertyImage> imageRepo,
            IPropertyCancellationSagaService sagaService,
            IMemoryCache cache,
            ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService webhookService,
            IAdminNotificationService adminNotificationService)
        {
            _propertyRepo = propertyRepo;
            _propertyReadOnlyRepo = propertyReadOnlyRepo;
            _categoryRepo = categoryRepo;
            _imageRepo = imageRepo;
            _sagaService = sagaService;
            _cache = cache;
            _webhookService = webhookService;
            _adminNotificationService = adminNotificationService;
        }

        private async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return 0;
            
            var categories = await _categoryRepo.GetAllAsync();
            var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            
            if (category == null)
            {
                category = new PropertyCategory { Name = categoryName };
                await _categoryRepo.AddAsync(category);
            }
            return category.Id;
        }

        public async Task<IEnumerable<Models.Property>> GetAllPropertiesAsync()
        {
            return await _propertyReadOnlyRepo.GetAsync(p => true, "Category,Images");
        }

        public async Task<Models.Property> GetPropertyByIdAsync(int id)
        {
            var properties = await _propertyReadOnlyRepo.GetAsync(p => p.Id == id, "Category,Images");
            var property = properties.FirstOrDefault();
            if (property == null) throw new Exception("Property not found");
            return property;
        }

        public async Task<Models.Property> AddPropertyAsync(PropertyCreateDto req, int adminId)
        {
            return await AddPropertyAsync(req);
        }

        public async Task<Models.Property> UpdatePropertyAsync(int id, PropertyCreateDto req)
        {
            var property = await GetPropertyByIdAsync(id);
            property.Title = req.Title;
            property.Description = req.Description;
            property.Price = req.Price;
            
            property.Address = req.Address;
            property.CarpetArea = req.CarpetArea;
            property.Facing = req.Facing;
            property.ProjectName = req.ProjectName;
            property.Bedrooms = req.Bedrooms;
            property.Bathrooms = req.Bathrooms;
            property.AreaSize = req.AreaSize;
            property.Furnishing = req.Furnishing;
            property.TotalFloors = req.TotalFloors;
            property.Maintenance = req.Maintenance;
            property.CarParking = req.CarParking;
            
            if (!string.IsNullOrWhiteSpace(req.Category))
            {
                property.CategoryId = await GetOrCreateCategoryIdAsync(req.Category);
            }

            await _propertyRepo.UpdateAsync(property);
            return property;
        }

        public async Task DeletePropertyAsync(int id)
        {
            // Initiate saga to cancel bookings and soft delete property
            await _sagaService.InitiatePropertyCancellationAsync(id);
        }

        public async Task<IEnumerable<Models.Property>> SearchPropertiesAsync(string? query, string? category, decimal? minPrice, decimal? maxPrice)
        {
            // Get the current cache version to invalidate cached searches globally when needed
            var cacheVersion = _cache.GetOrCreate("PropertySearch_CacheVersion", entry => 
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return Guid.NewGuid().ToString();
            });

            var cacheKey = $"SearchProperties_{cacheVersion}_{query}_{category}_{minPrice}_{maxPrice}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<Models.Property>? cachedProperties))
            {
                return cachedProperties ?? new List<Models.Property>();
            }

            var properties = await _propertyReadOnlyRepo.GetAsync(p => true, "Category,Images");
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                properties = properties.Where(p => 
                    p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                    p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                properties = properties.Where(p => p.Category != null && p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (minPrice.HasValue)
            {
                properties = properties.Where(p => ApexWorld_Backend.Common.Constants.MonetaryConstants.IsGreaterThanOrEqual(p.Price, minPrice.Value)).ToList();
            }

            if (maxPrice.HasValue)
            {
                properties = properties.Where(p => ApexWorld_Backend.Common.Constants.MonetaryConstants.IsLessThanOrEqual(p.Price, maxPrice.Value)).ToList();
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(cacheKey, properties, cacheOptions);

            return properties;
        }
        
        // IPropertyQueryService
        public async Task<List<Models.Property>> GetListedPropertiesAsync(string? category)
        {
            var properties = await _propertyReadOnlyRepo.GetAsync(p => p.IsAvailable && p.Status == "Available", "Category,Images");
            
            if (!string.IsNullOrWhiteSpace(category))
            {
                properties = properties.Where(p => p.Category != null && p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            return properties.ToList();
        }

        public async Task<Models.Property> GetPropertyDetailsAsync(int id)
        {
            var property = await GetPropertyByIdAsync(id);
            if (!property.IsAvailable || property.Status != "Available")
            {
                throw new Exception("Property is no longer available for viewing.");
            }
            return property;
        }
        
        // IPropertyCommandService
        public async Task<Models.Property> AddPropertyAsync(PropertyCreateDto req)
        {
            var property = new Models.Property
            {
                Title = req.Title,
                Description = req.Description,
                Price = req.Price,
                Status = "Pending",
                IsAvailable = true,
                Address = req.Address,
                CarpetArea = req.CarpetArea,
                Facing = req.Facing,
                ProjectName = req.ProjectName,
                Bedrooms = req.Bedrooms,
                Bathrooms = req.Bathrooms,
                AreaSize = req.AreaSize,
                Furnishing = req.Furnishing,
                TotalFloors = req.TotalFloors,
                Maintenance = req.Maintenance,
                CarParking = req.CarParking
            };

            if (!string.IsNullOrWhiteSpace(req.Category))
            {
                property.CategoryId = await GetOrCreateCategoryIdAsync(req.Category);
            }

            await _propertyRepo.AddAsync(property);
            
            if (req.ImageUrls != null && req.ImageUrls.Any())
            {
                foreach (var url in req.ImageUrls)
                {
                    await _imageRepo.AddAsync(new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = url
                    });
                }
            }
            
            try
            {
                // Webhook trigger
                await _webhookService.EnqueueEventAsync("Property.Created", property);
                
                await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
                {
                    Title = "New Property Listing Pending Approval",
                    Message = $"A new property '{property.Title}' has been added and requires your approval to go live.",
                    Category = "Properties",
                    TargetAudience = "SpecificRole",
                    TargetRole = "Admin"
                }, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddProperty] Post-save side-effect failed (webhook/notification): {ex.Message}");
            }
            
            return await GetPropertyByIdAsync(property.Id);
        }

        public async Task<Models.Property> UpdatePropertyAsync(int id, PropertyUpdateDto req)
        {
            var property = await GetPropertyByIdAsync(id);
            property.Title = req.Title;
            property.Description = req.Description;
            property.Price = req.Price;
            
            property.ProjectName = req.ProjectName;
            property.Furnishing = req.Furnishing;
            property.TotalFloors = req.TotalFloors;
            property.Maintenance = req.Maintenance;

            await _propertyRepo.UpdateAsync(property);
            return property;
        }

        public async Task<Models.Property> UpdatePropertyStatusAsync(int id, PropertyStatusUpdateDto req)
        {
            var property = await GetPropertyByIdAsync(id);
            property.Status = req.Status;
            property.IsAvailable = req.IsAvailable;
            
            await _propertyRepo.UpdateAsync(property);
            
            try 
            {
                // Webhook trigger
                await _webhookService.EnqueueEventAsync("Property.StatusChanged", property);
                
                if (req.Status == "Available" || req.Status == "Rejected")
                {
                    await _adminNotificationService.BroadcastNotificationAsync(new BroadcastNotificationDto
                    {
                        Title = $"Property {req.Status}",
                        Message = $"The property listing '{property.Title}' has been {req.Status.ToLower()}.",
                        Category = "Properties",
                        TargetAudience = "SpecificRole",
                        TargetRole = "SubAdmin"
                    }, 0); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in post-update triggers: {ex.Message}");
            }
            
            return property;
        }
    }
}
