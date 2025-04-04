using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebChatBot.Data;
using WebChatBot.Models;

namespace WebChatBot.Services
{
    public class TimeSlotGenerator
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate time slots for doctors for the next specified number of days
        /// </summary>
        public async Task GenerateTimeSlotsAsync(int daysAhead = 14)
        {
            // Get all active doctors
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            // Calculate start and end dates
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = startDate.AddDays(daysAhead);

            // Generate slots for each doctor
            foreach (var doctor in doctors)
            {
                for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
                {
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Generate morning slots
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 9, 0, 30); // 9:00 - 9:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 9, 30, 30); // 9:30 - 10:00
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 10, 0, 30); // 10:00 - 10:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 10, 30, 30); // 10:30 - 11:00
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 11, 0, 30); // 11:00 - 11:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 11, 30, 30); // 11:30 - 12:00

                    // Generate afternoon slots
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 13, 0, 30); // 1:00 - 1:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 13, 30, 30); // 1:30 - 2:00
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 14, 0, 30); // 2:00 - 2:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 14, 30, 30); // 2:30 - 3:00
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 15, 0, 30); // 3:00 - 3:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 15, 30, 30); // 3:30 - 4:00
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 16, 0, 30); // 4:00 - 4:30
                    await CreateSlotIfNotExistsAsync(doctor.DoctorId, date, 16, 30, 30); // 4:30 - 5:00
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Create a single time slot if it doesn't already exist
        /// </summary>
        private async Task CreateSlotIfNotExistsAsync(int doctorId, DateTime date, int hour, int minute, int durationMinutes)
        {
            // Make sure we're using UTC
            DateTime slotStart = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Utc);
            DateTime slotEnd = slotStart.AddMinutes(durationMinutes);

            // Check if the slot already exists
            bool slotExists = await _context.TimeSlots.AnyAsync(ts => 
                ts.DoctorId == doctorId && 
                ts.StartTime == slotStart && 
                ts.EndTime == slotEnd);

            if (!slotExists)
            {
                _context.TimeSlots.Add(new TimeSlot
                {
                    DoctorId = doctorId,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = true
                });
            }
        }
    }
}