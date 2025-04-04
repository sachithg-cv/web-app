using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChatBot.Models
{
    public class ChatMessage
    {
        public int Id {get; set; }

        public string SessionId {get; set; }

        public string message { get; set; }

        public string sender { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}