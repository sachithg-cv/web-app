// Make sure to include these using statements
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WebChatBot.Models; 

namespace WebChatBot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages;

        // Define your DbSet properties for your models here
        // public DbSet<YourModel> YourModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ChatMessage>(entity => 
            {
                // Configure Id as auto-incrementing primary key
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .UseIdentityColumn() // This sets up auto-increment
                    .ValueGeneratedOnAdd();
                
                // Keep your existing index on SessionId
                entity.HasIndex(m => m.SessionId);
                
                // Optional: configure other properties
                entity.Property(e => e.message).IsRequired();
                entity.Property(e => e.sender).IsRequired();
            });
        }
    }
}