using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebChatBot.Data;
using WebChatBot.Models;

namespace WebChatBot.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatService(ApplicationDbContext dbContext)
        {
            // Add null check to debug the issue
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext), "Database context is null in ChatService constructor");
            }
            
            _dbContext = dbContext;
            
            // Verify DbSet is not null
            if (_dbContext.ChatMessages == null)
            {
                throw new InvalidOperationException("ChatMessages DbSet is null. Check your ApplicationDbContext configuration.");
            }
        }

        public async Task AddMessageAsync(string sessionId, string message, string sender)
        {
            var chatMessage = new ChatMessage {
                SessionId= sessionId,
                Message = message,
                Sender = sender,
                Timestamp = DateTime.UtcNow
            };
            
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();
        }

         public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string sessionId)
        {
            return await _dbContext.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
        }
    }
}