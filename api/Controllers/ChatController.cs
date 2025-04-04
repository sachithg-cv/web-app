using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebChatBot.Services;
using Microsoft.AspNetCore.Cors;

namespace WebChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    public class ChatController : ControllerBase
    {
        private readonly SemanticKernelService _semanticKernelService;

        private readonly ChatService _chatService;

        public ChatController(SemanticKernelService semanticKernelService, ChatService chatService)
        {
            _semanticKernelService = semanticKernelService;
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<ActionResult> GetChatMessageAsync(string sessionId) {
            var messages = await _chatService.GetMessagesAsync(sessionId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult> GetChatResponseAsync([FromBody] ChatRequest request)
        {
            var messages = await _chatService.GetMessagesAsync(request.SessionId);
            await _chatService.AddMessageAsync(request.SessionId,request.Message, "user");
            var response = await _semanticKernelService.GetChatResponseAsync(request.Message, messages);
            await _chatService.AddMessageAsync(request.SessionId,response, "system");
            return Ok(response);
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}