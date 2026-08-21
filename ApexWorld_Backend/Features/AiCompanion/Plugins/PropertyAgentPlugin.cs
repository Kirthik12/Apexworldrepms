using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using ApexWorld_Backend.Features.Property.Services;
using ApexWorld_Backend.Features.Booking.Services;
using ApexWorld_Backend.Features.Booking.DTOs;

namespace ApexWorld_Backend.Features.AiCompanion.Plugins
{
    public class PropertyAgentPlugin
    {
        private readonly IPropertyQueryService _propertyQueryService;
        private readonly IBookingService _bookingService;

        public PropertyAgentPlugin(IPropertyQueryService propertyQueryService, IBookingService bookingService)
        {
            _propertyQueryService = propertyQueryService;
            _bookingService = bookingService;
        }

        [KernelFunction, Description("Gets full details of a property by its property ID including BHK, bathrooms, size, pricing, facing, address, and amenities.")]
        public async Task<string> GetPropertyDetails(
            [Description("The unique integer ID of the property listing")] int propertyId)
        {
            try
            {
                var prop = await _propertyQueryService.GetPropertyDetailsAsync(propertyId);
                if (prop == null)
                {
                    return $"Property with ID {propertyId} was not found.";
                }

                return $"Property Details for ID {propertyId}:\n" +
                       $"- Title: {prop.Title}\n" +
                       $"- Project Name: {prop.ProjectName}\n" +
                       $"- Description: {prop.Description}\n" +
                       $"- Price: INR {prop.Price}\n" +
                       $"- Address: {prop.Address}\n" +
                       $"- Carpet Area: {prop.CarpetArea} sqft\n" +
                       $"- Facing: {prop.Facing}\n" +
                       $"- Bedrooms: {prop.Bedrooms} BHK\n" +
                       $"- Bathrooms: {prop.Bathrooms}\n" +
                       $"- Furnishing: {prop.Furnishing}\n" +
                       $"- Total Floors: {prop.TotalFloors}\n" +
                       $"- Monthly Maintenance: INR {prop.Maintenance}\n" +
                       $"- Car Parking: {prop.CarParking} spots\n" +
                       $"- Is Available: {prop.IsAvailable}\n" +
                       $"- Status: {prop.Status}";
            }
            catch (Exception ex)
            {
                return $"Error retrieving property details: {ex.Message}";
            }
        }

        [KernelFunction, Description("Retrieves available site-visit time slots for a given property.")]
        public async Task<List<string>> GetAvailableSiteVisitSlots(
            [Description("The unique integer ID of the property")] int propertyId)
        {
            // Generate scheduling slots for the next 3 days starting tomorrow
            var slots = new List<string>();
            var today = DateTime.Today;
            for (int i = 1; i <= 3; i++)
            {
                var nextDay = today.AddDays(i);
                slots.Add(nextDay.ToString("yyyy-MM-dd") + " 10:00 AM");
                slots.Add(nextDay.ToString("yyyy-MM-dd") + " 02:00 PM");
                slots.Add(nextDay.ToString("yyyy-MM-dd") + " 04:00 PM");
            }
            return await Task.FromResult(slots);
        }

        [KernelFunction, Description("Initiates or schedules a site visit / booking for a property.")]
        public async Task<string> BookSiteVisit(
            [Description("The unique integer ID of the property")] int propertyId,
            [Description("The buyer ID of the user booking the visit")] int buyerId,
            [Description("The date and time of the scheduled visit, e.g. '2026-08-22 10:00 AM'")] string scheduledDateTime,
            [Description("First name of the buyer")] string firstName,
            [Description("Last name of the buyer")] string lastName,
            [Description("Email of the buyer")] string email,
            [Description("Phone number of the buyer")] string phone)
        {
            try
            {
                if (!DateTime.TryParse(scheduledDateTime, out DateTime parsedDateTime))
                {
                    return "Invalid date format. Please specify the date in 'yyyy-MM-dd hh:mm tt' format (e.g., '2026-08-22 10:00 AM').";
                }

                var request = new BookingRequestDto
                {
                    PropertyId = propertyId,
                    BuyerId = buyerId,
                    ScheduledDate = parsedDateTime,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PhoneNumber = phone,
                    PermanentAddress = "N/A"
                };

                var booking = await _bookingService.InitiateBookingAsync(request);
                return $"SUCCESS: Site visit successfully scheduled for {parsedDateTime:g}. Booking ID is {booking.Id}. Status is {booking.Status}.";
            }
            catch (Exception ex)
            {
                return $"FAILED: Could not schedule site visit: {ex.Message}";
            }
        }

        [KernelFunction, Description("Searches and suggests properties in the database based on filters like search query text, category, min price, and max price.")]
        public async Task<string> SearchProperties(
            [Description("Optional search text query for property title or description")] string? query = null,
            [Description("Optional property category name, e.g., 'Villa', 'Apartment', 'Commercial'")] string? category = null,
            [Description("Optional minimum price range")] decimal? minPrice = null,
            [Description("Optional maximum price range")] decimal? maxPrice = null)
        {
            try
            {
                var properties = await _propertyQueryService.SearchPropertiesAsync(query, category, minPrice, maxPrice);
                var list = new List<string>();
                foreach (var prop in properties)
                {
                    list.Add($"- ID: {prop.Id}, Title: {prop.Title}, Price: INR {prop.Price}, Location: {prop.Address}, BHK: {prop.Bedrooms} BHK");
                }

                if (list.Count == 0)
                {
                    return "No matching properties were found in the database for the given search criteria.";
                }

                return "Matching Properties Found:\n" + string.Join("\n", list);
            }
            catch (Exception ex)
            {
                return $"Error searching properties: {ex.Message}";
            }
        }
    }
}
