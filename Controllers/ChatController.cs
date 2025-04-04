using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebChatBot.Services;

namespace WebChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var sessionId = HttpContext.Session.Id;
            await _chatService.AddMessageAsync(sessionId,request.Message, "user");
            var response = await _semanticKernelService.GetChatResponseAsync(request.Message);
            await _chatService.AddMessageAsync(sessionId,response, "system");
            return Ok(response);
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}