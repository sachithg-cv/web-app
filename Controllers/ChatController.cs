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

        public ChatController(SemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpGet]
        public ActionResult<string> Index() {
            return "Hello world";
        }

        [HttpPost]
        public async Task<ActionResult> GetChatResponseAsync([FromBody] ChatRequest request)
        {
            var response = await _semanticKernelService.GetChatResponseAsync(request.Message);
            return Ok(response);
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}