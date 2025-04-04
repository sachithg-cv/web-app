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

        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ChatMessage>(entity => 
            {
                // Configure Id as auto-incrementing primary key
                entity.ToTable("ChatMessage"); 
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .UseIdentityColumn() // This sets up auto-increment
                    .ValueGeneratedOnAdd();
                
                // Keep your existing index on SessionId
                entity.HasIndex(m => m.SessionId);
                
                // Optional: configure other properties
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Sender).IsRequired();

                 modelBuilder.Entity<TimeSlot>()
                .HasIndex(ts => new { ts.DoctorId, ts.StartTime, ts.EndTime })
                .IsUnique();
                
            // Configure one-to-one relationship between TimeSlot and Appointment
            modelBuilder.Entity<TimeSlot>()
                .HasOne(ts => ts.Appointment)
                .WithOne(a => a.Slot)
                .HasForeignKey<Appointment>(a => a.SlotId);

            // Configure relationships between entities
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId);

            modelBuilder.Entity<TimeSlot>()
                .HasOne(ts => ts.Doctor)
                .WithMany(d => d.TimeSlots)
                .HasForeignKey(ts => ts.DoctorId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId);
            });
        }
    }
}