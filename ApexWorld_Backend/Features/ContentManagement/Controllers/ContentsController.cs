using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.ContentManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.ContentManagement.Controllers
{
    [Tags("Admin - Content Management")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ContentsController : ControllerBase
    {
        private readonly IRepository<Content> _repository;

        public ContentsController(IRepository<Content> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> GetAllContents()
        {
            var contents = await _repository.GetAllAsync();
            return Ok(ApiResponse<object>.SuccessResponse(contents));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> GetContentById(int id)
        {
            var content = await _repository.GetByIdAsync(id);
            if (content == null) return NotFound(ApiResponse<string>.ErrorResponse("Content not found"));
            return Ok(ApiResponse<object>.SuccessResponse(content));
        }

        [HttpPost]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> CreateContent([FromBody] Content content)
        {
            var createdContent = await _repository.AddAsync(content);
            return Ok(ApiResponse<object>.SuccessResponse(createdContent));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> UpdateContent(int id, [FromBody] Content contentUpdate)
        {
            var existingContent = await _repository.GetByIdAsync(id);
            if (existingContent == null) return NotFound(ApiResponse<string>.ErrorResponse("Content not found"));

            existingContent.Section = contentUpdate.Section;
            existingContent.Key = contentUpdate.Key;
            existingContent.Value = contentUpdate.Value;
            existingContent.ContentType = contentUpdate.ContentType;
            existingContent.IsActive = contentUpdate.IsActive;

            await _repository.UpdateAsync(existingContent);
            return Ok(ApiResponse<object>.SuccessResponse(existingContent));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var existingContent = await _repository.GetByIdAsync(id);
            if (existingContent == null) return NotFound(ApiResponse<string>.ErrorResponse("Content not found"));

            await _repository.DeleteAsync(existingContent);
            return Ok(ApiResponse<string>.SuccessResponse("Content deleted successfully"));
        }

        [HttpGet("filter")]
        [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
        public async Task<IActionResult> FilterContents([FromQuery] string? section, [FromQuery] string? key)
        {
            var allContents = await _repository.GetAllAsync();
            var query = allContents.AsQueryable();

            if (!string.IsNullOrEmpty(section))
            {
                query = query.Where(c => c.Section.Equals(section, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(key))
            {
                query = query.Where(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(ApiResponse<object>.SuccessResponse(query.ToList()));
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicContents()
        {
            var allContents = await _repository.GetAllAsync();
            var activeContents = allContents.Where(c => c.IsActive).ToList();
            return Ok(ApiResponse<object>.SuccessResponse(activeContents));
        }
    }
}
