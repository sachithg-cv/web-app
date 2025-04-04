using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebChatBot.Data;
using WebChatBot.Models;
using WebChatBot.Services;

namespace WebChatBot.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Apply any pending migrations
                await context.Database.MigrateAsync();
                
                // Seed specialties if none exist
                if (!context.Specialties.Any())
                {
                    await SeedSpecialties(context);
                }
                
                // Seed doctors if none exist
                if (!context.Doctors.Any())
                {
                    await SeedDoctors(context);
                }
                
                // Generate time slots
                var slotGenerator = scope.ServiceProvider.GetRequiredService<TimeSlotGenerator>();
                await slotGenerator.GenerateTimeSlotsAsync(14); // Generate slots for the next 14 days
            }
        }
        
        private static async Task SeedSpecialties(ApplicationDbContext context)
        {
            var specialties = new[]
            {
                new Specialty { Name = "General Practice", Description = "General medical consultation and primary care" },
                new Specialty { Name = "Cardiology", Description = "Heart-related conditions" },
                new Specialty { Name = "Dermatology", Description = "Skin conditions and treatments" },
                new Specialty { Name = "Orthopedics", Description = "Bone and joint care" },
                new Specialty { Name = "Pediatrics", Description = "Child healthcare" },
                new Specialty { Name = "Neurology", Description = "Nervous system disorders" },
                new Specialty { Name = "Gynecology", Description = "Women's healthcare" },
                new Specialty { Name = "Ophthalmology", Description = "Eye care and vision" },
                new Specialty { Name = "ENT", Description = "Ear, Nose, and Throat specialists" }
            };
            
            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();
        }
        
        private static async Task SeedDoctors(ApplicationDbContext context)
        {
            var doctors = new[]
            {
                new Doctor { Name = "Dr. Smith", SpecialtyId = 1 }, // General Practice
                new Doctor { Name = "Dr. Johnson", SpecialtyId = 2 }, // Cardiology
                new Doctor { Name = "Dr. Williams", SpecialtyId = 3 }, // Dermatology
                new Doctor { Name = "Dr. Brown", SpecialtyId = 4 }, // Orthopedics
                new Doctor { Name = "Dr. Jones", SpecialtyId = 5 }, // Pediatrics
                new Doctor { Name = "Dr. Garcia", SpecialtyId = 6 }, // Neurology
                new Doctor { Name = "Dr. Miller", SpecialtyId = 7 }, // Gynecology
                new Doctor { Name = "Dr. Davis", SpecialtyId = 8 }, // Ophthalmology
                new Doctor { Name = "Dr. Rodriguez", SpecialtyId = 9 } // ENT
            };
            
            context.Doctors.AddRange(doctors);
            await context.SaveChangesAsync();
        }
    }
}