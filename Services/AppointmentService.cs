using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebChatBot.Data;
using WebChatBot.Models;

namespace WebChatBot.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all available specialties
        /// </summary>
        public async Task<List<Specialty>> GetSpecialtiesAsync()
        {
            return await _context.Specialties
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Check if a specialty exists by name (case insensitive)
        /// </summary>
        public async Task<bool> SpecialtyExistsAsync(string specialtyName)
        {
            return await _context.Specialties
                .AnyAsync(s => s.Name.ToLower() == specialtyName.ToLower() && s.IsActive);
        }

        /// <summary>
        /// Get available time slots for a specific specialty
        /// </summary>
        public async Task<List<AvailableSlot>> GetAvailableSlotsAsync(string specialtyName, DateTime? fromDate = null)
        {
            // Ensure dates are in UTC format for PostgreSQL
            if (fromDate == null)
                fromDate = DateTime.UtcNow.Date;
            else if (fromDate.Value.Kind != DateTimeKind.Utc)
                fromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

            try
            {
                var availableSlots = await _context.TimeSlots
                    .Include(ts => ts.Doctor)
                    .ThenInclude(d => d.Specialty)
                    .Where(ts => 
                        ts.IsAvailable && 
                        ts.Doctor.IsActive && 
                        ts.Doctor.Specialty.IsActive &&
                        ts.Doctor.Specialty.Name.ToLower() == specialtyName.ToLower() &&
                        ts.StartTime >= fromDate)
                    .OrderBy(ts => ts.StartTime)
                    .Take(20) // Limit results
                    .Select(ts => new AvailableSlot
                    {
                        SlotId = ts.SlotId,
                        DoctorName = ts.Doctor.Name,
                        SpecialtyName = ts.Doctor.Specialty.Name,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        FormattedDateTime = $"{ts.StartTime.ToString("dddd, MMMM d, yyyy")} at {ts.StartTime.ToString("h:mm tt")} - {ts.EndTime.ToString("h:mm tt")}"
                    })
                    .ToListAsync();

                return availableSlots;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching available slots: {ex.Message}");
                return new List<AvailableSlot>();
            }
        }

        /// <summary>
        /// Book an appointment for a patient
        /// </summary>
        public async Task<BookingResult> BookAppointmentAsync(AppointmentRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if the slot exists and is available
                var slot = await _context.TimeSlots
                    .FirstOrDefaultAsync(ts => ts.SlotId == request.SlotId && ts.IsAvailable);

                if (slot == null)
                {
                    return new BookingResult
                    {
                        Success = false,
                        Message = "The selected time slot is no longer available."
                    };
                }

                // Find or create patient
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.ContactInfo == request.ContactInfo);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        Name = request.PatientName,
                        ContactInfo = request.ContactInfo,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }
                else if (patient.Name != request.PatientName)
                {
                    // Update patient name if it has changed
                    patient.Name = request.PatientName;
                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                }

                // Generate confirmation code
                string confirmationCode = GenerateConfirmationCode();

                // Create appointment
                var appointment = new Appointment
                {
                    PatientId = patient.PatientId,
                    SlotId = slot.SlotId,
                    Reason = request.Reason,
                    Status = "confirmed",
                    ConfirmationCode = confirmationCode,
                    BookedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);

                // Mark the slot as no longer available
                slot.IsAvailable = false;
                _context.TimeSlots.Update(slot);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Get doctor and specialty details for the response
                var slotDetails = await _context.TimeSlots
                    .Include(ts => ts.Doctor)
                    .ThenInclude(d => d.Specialty)
                    .FirstOrDefaultAsync(ts => ts.SlotId == slot.SlotId);

                return new BookingResult
                {
                    Success = true,
                    AppointmentId = appointment.AppointmentId,
                    ConfirmationCode = confirmationCode,
                    PatientName = patient.Name,
                    DoctorName = slotDetails.Doctor.Name,
                    SpecialtyName = slotDetails.Doctor.Specialty.Name,
                    DateTime = $"{slot.StartTime.ToString("dddd, MMMM d, yyyy")} at {slot.StartTime.ToString("h:mm tt")} - {slot.EndTime.ToString("h:mm tt")}",
                    Reason = request.Reason,
                    Message = "Appointment booked successfully!"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BookingResult
                {
                    Success = false,
                    Message = $"Error booking appointment: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate a random confirmation code
        /// </summary>
        private string GenerateConfirmationCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Removed potentially confusing characters like I, O, 0, 1
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get appointment details by confirmation code
        /// </summary>
        public async Task<BookingResult> GetAppointmentByCodeAsync(string confirmationCode)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .ThenInclude(s => s.Doctor)
                .ThenInclude(d => d.Specialty)
                .FirstOrDefaultAsync(a => a.ConfirmationCode == confirmationCode);

            if (appointment == null)
            {
                return new BookingResult
                {
                    Success = false,
                    Message = "Appointment not found."
                };
            }

            return new BookingResult
            {
                Success = true,
                AppointmentId = appointment.AppointmentId,
                ConfirmationCode = appointment.ConfirmationCode,
                PatientName = appointment.Patient.Name,
                DoctorName = appointment.Slot.Doctor.Name,
                SpecialtyName = appointment.Slot.Doctor.Specialty.Name,
                DateTime = $"{appointment.Slot.StartTime.ToString("dddd, MMMM d, yyyy")} at {appointment.Slot.StartTime.ToString("h:mm tt")} - {appointment.Slot.EndTime.ToString("h:mm tt")}",
                Reason = appointment.Reason,
                Status = appointment.Status
            };
        }
    }

    // DTO classes for appointment operations
    public class AvailableSlot
    {
        public int SlotId { get; set; }
        public string DoctorName { get; set; }
        public string SpecialtyName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string FormattedDateTime { get; set; }
    }

    public class AppointmentRequest
    {
        public string PatientName { get; set; }
        public string ContactInfo { get; set; }
        public int SlotId { get; set; }
        public string Reason { get; set; }
    }

    public class BookingResult
    {
        public bool Success { get; set; }
        public int AppointmentId { get; set; }
        public string ConfirmationCode { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string SpecialtyName { get; set; }
        public string DateTime { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}