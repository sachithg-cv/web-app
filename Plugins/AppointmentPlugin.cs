using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using WebChatBot.Models;

namespace WebChatBot.Plugins
{
    public class AppointmentPlugin
    {
        private readonly Kernel _kernel;

        // In a real application, this would be a database repository
        private static List<AppointmentDetails> _appointments = new List<AppointmentDetails>();

        public AppointmentPlugin(Kernel kernel)
        {
            _kernel = kernel;
        }

        [KernelFunction, Description("Extract appointment details from conversation")]
        public async Task<string> ExtractAppointmentDetails(
            [Description("The full conversation text between the user and assistant")]
            string conversationText)
        {
            // Create a function that extracts appointment details from conversation
            KernelFunction extractorFunction = _kernel.CreateFunctionFromPrompt(
                @"Based on the conversation, extract the appointment details as accurately as possible.
                Return a JSON object with these fields:
                {
                    ""patientName"": ""name or unknown"",
                    ""specialty"": ""medical specialty or unknown"",
                    ""dateTime"": ""date and time or unknown"",
                    ""reason"": ""reason for visit or unknown"",
                    ""contactInfo"": ""contact information or unknown""
                }
                Only include information explicitly mentioned in the conversation.
                If information isn't provided, use ""unknown"" as the value.
                Return ONLY valid JSON, no other text.",
                
                new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings { 
                    Temperature = 0.0,
                    //ResponseFormat = "json_object"
                });
                
            // Execute the extractor function with the conversation text
            var result = await extractorFunction.InvokeAsync(_kernel, new KernelArguments { ["input"] = conversationText });
            
            return result.GetValue<string>();
        }

        [KernelFunction, Description("Validate appointment details for completeness")]
        public string ValidateAppointment(
            [Description("JSON string containing appointment details")]
            string appointmentJson)
        {
            try
            {
                var appointment = JsonSerializer.Deserialize<AppointmentDetails>(appointmentJson);
                
                // Create validation messages for missing fields
                var missingFields = new List<string>();
                
                if (appointment.PatientName == "unknown") missingFields.Add("patient name");
                if (appointment.Specialty == "unknown") missingFields.Add("doctor specialty");
                if (appointment.DateTime == "unknown") missingFields.Add("preferred date and time");
                if (appointment.Reason == "unknown") missingFields.Add("reason for visit");
                if (appointment.ContactInfo == "unknown") missingFields.Add("contact information");

                // If no missing fields, appointment is valid
                if (missingFields.Count == 0)
                {
                    return JsonSerializer.Serialize(new { 
                        isValid = true, 
                        message = "All appointment details are complete.",
                        appointment = appointment
                    });
                }
                
                // Otherwise, return what's missing
                return JsonSerializer.Serialize(new { 
                    isValid = false, 
                    missingFields = missingFields,
                    message = $"The following details are still needed: {string.Join(", ", missingFields)}.",
                    appointment = appointment
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { 
                    isValid = false, 
                    message = $"Error validating appointment: {ex.Message}"
                });
            }
        }

        [KernelFunction, Description("Book an appointment and return confirmation details")]
        public string BookAppointment(
            [Description("JSON string containing validated appointment details")]
            string appointmentJson)
        {
            try
            {
                var appointment = JsonSerializer.Deserialize<AppointmentDetails>(appointmentJson);
                
                // Generate a unique appointment ID
                appointment.Id = Guid.NewGuid().ToString();
                appointment.Status = "confirmed";
                appointment.ConfirmationTime = DateTime.UtcNow;
                
                // In a real application, save to database
                _appointments.Add(appointment);
                
                // Return confirmation info
                return JsonSerializer.Serialize(new {
                    success = true,
                    confirmationCode = appointment.Id.Substring(0, 8).ToUpper(),
                    message = "Appointment booked successfully.",
                    appointment = appointment
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new {
                    success = false,
                    message = $"Failed to book appointment: {ex.Message}"
                });
            }
        }

        [KernelFunction, Description("Check available appointment slots for a specialty")]
        public string CheckAvailability(
            [Description("The medical specialty to check availability for")]
            string specialty,
            [Description("The preferred date (optional)")]
            string date = "")
        {
            // In a real application, this would query a database
            // For demo purposes, we'll return mock data
            
            var availableSlots = new List<Dictionary<string, string>>();
            
            // Generate some mock availability data
            DateTime startDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today.AddDays(1) 
                : DateTime.Parse(date);
                
            for (int day = 0; day < 5; day++)
            {
                DateTime currentDate = startDate.AddDays(day);
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                    
                // Add morning slots
                availableSlots.Add(new Dictionary<string, string> {
                    ["date"] = currentDate.ToString("yyyy-MM-dd"),
                    ["day"] = currentDate.DayOfWeek.ToString(),
                    ["time"] = "09:00 AM"
                });
                
                availableSlots.Add(new Dictionary<string, string> {
                    ["date"] = currentDate.ToString("yyyy-MM-dd"),
                    ["day"] = currentDate.DayOfWeek.ToString(),
                    ["time"] = "11:30 AM"
                });
                
                // Add afternoon slots
                availableSlots.Add(new Dictionary<string, string> {
                    ["date"] = currentDate.ToString("yyyy-MM-dd"),
                    ["day"] = currentDate.DayOfWeek.ToString(),
                    ["time"] = "2:00 PM"
                });
                
                availableSlots.Add(new Dictionary<string, string> {
                    ["date"] = currentDate.ToString("yyyy-MM-dd"),
                    ["day"] = currentDate.DayOfWeek.ToString(),
                    ["time"] = "4:30 PM"
                });
            }
            
            return JsonSerializer.Serialize(new {
                specialty = specialty,
                availableSlots = availableSlots
            });
        }
    }
}