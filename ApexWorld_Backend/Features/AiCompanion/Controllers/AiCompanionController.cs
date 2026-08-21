using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Models;

namespace ApexWorld_Backend.Features.AiCompanion.Controllers
{
    [Tags("Buyer - AI Companion")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Buyer)]
    public class AiCompanionController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly ICurrentUserService _currentUserService;

        public AiCompanionController(Kernel kernel, ICurrentUserService currentUserService)
        {
            _kernel = kernel;
            _currentUserService = currentUserService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            try
            {
                var buyerIdStr = _currentUserService.UserId;
                if (string.IsNullOrEmpty(buyerIdStr) || !int.TryParse(buyerIdStr, out int buyerId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Buyer is not authenticated or invalid user ID."));
                }

                var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

                // Configure Chat History
                var chatHistory = new ChatHistory();
                
                // Construct System Message. Provide instructions to automatically use the property search tool if needed.
                var propertyContextText = request.PropertyId.HasValue 
                    ? $"The current property ID being viewed is {request.PropertyId.Value}." 
                    : "The user is currently browsing the portal generally and not viewing any specific property.";

                chatHistory.AddSystemMessage(
                    "You are the ApexWorld AI Property Companion. You help buyers learn about listings, search/suggest properties, and schedule site visits.\n" +
                    $"Note: The current buyer's ID is {buyerId}. {propertyContextText}\n" +
                    "Use the available tools to fetch details for this property if the buyer asks details about it. " +
                    "Use the 'SearchProperties' tool to search and suggest properties from the database if they ask to find or recommend properties matching criteria (like price range, category, location, etc.).\n" +
                    "If the property details are not mentioned in the database or visible in the front photo, politely explain that you don't know and encourage them to schedule a site visit.\n" +
                    "If they want to book a site-visit, ask them for their details (first name, last name, email, phone) if not already provided, and use the 'BookSiteVisit' tool to schedule it."
                );

                // Add past conversation history
                if (request.ChatHistory != null)
                {
                    foreach (var msg in request.ChatHistory)
                    {
                        if (msg.Sender.ToLower() == "user")
                        {
                            chatHistory.AddUserMessage(msg.Text);
                        }
                        else
                        {
                            chatHistory.AddAssistantMessage(msg.Text);
                        }
                    }
                }

                // Add current message (text + image if multimodal)
                if (!string.IsNullOrEmpty(request.ImageBase64))
                {
                    byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);
                    chatHistory.AddUserMessage(new ChatMessageContentItemCollection
                    {
                        new TextContent(request.Message),
                        new ImageContent(new ReadOnlyMemory<byte>(imageBytes), "image/jpeg")
                    });
                }
                else
                {
                    chatHistory.AddUserMessage(request.Message);
                }

                // Configure function choice behavior to allow the model to use tools
                var settings = new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                // Get AI response
                var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, settings, _kernel);

                return Ok(ApiResponse<ChatResponseDto>.SuccessResponse(new ChatResponseDto
                {
                    ReplyText = response.Content ?? "I'm sorry, I could not generate a response."
                }, "Response generated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"AI Assistant Error: {ex.ToString()}"));
            }
        }
    }

    public class ChatRequestDto
    {
        public int? PropertyId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        public List<ChatMessageDto>? ChatHistory { get; set; }
    }

    public class ChatMessageDto
    {
        public string Sender { get; set; } = "user"; // "user" or "ai"
        public string Text { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string ReplyText { get; set; } = string.Empty;
    }
}
