using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using WebChatBot.Models;
using WebChatBot.Services;

namespace WebChatBot.Plugins
{
    public class AppointmentPlugin
    {
        private readonly Kernel _kernel;
        private readonly AppointmentService _appointmentService;

        public AppointmentPlugin(Kernel kernel, AppointmentService appointmentService)
        {
            _kernel = kernel;
            _appointmentService = appointmentService;
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
                    Temperature = 0.0
                    // ResponseFormat = "json_object" - commented out to avoid preview API warning
                });

            // Execute the extractor function with the conversation text
            var result = await extractorFunction.InvokeAsync(_kernel, new KernelArguments { ["input"] = conversationText });
            var response = result.GetValue<string>();
            
            // Ensure we have valid JSON
            try {
                JsonDocument.Parse(response);
                return response;
            } catch (JsonException) {
                // If not valid JSON, try to extract just the JSON part
                int start = response.IndexOf('{');
                int end = response.LastIndexOf('}');
                
                if (start >= 0 && end > start) {
                    string jsonPart = response.Substring(start, end - start + 1);
                    try {
                        JsonDocument.Parse(jsonPart);
                        return jsonPart;
                    } catch (JsonException) {
                        // Return default
                        return "{ \"patientName\": \"unknown\", \"specialty\": \"unknown\", \"dateTime\": \"unknown\", \"reason\": \"unknown\", \"contactInfo\": \"unknown\" }";
                    }
                }
                
                // Default
                return "{ \"patientName\": \"unknown\", \"specialty\": \"unknown\", \"dateTime\": \"unknown\", \"reason\": \"unknown\", \"contactInfo\": \"unknown\" }";
            }
        }

        [KernelFunction, Description("Validate appointment details for completeness")]
        public async Task<string> ValidateAppointment(
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

                // If specialty is provided, validate that it exists
                bool specialtyValid = true;
                if (appointment.Specialty != "unknown")
                {
                    specialtyValid = await _appointmentService.SpecialtyExistsAsync(appointment.Specialty);
                    if (!specialtyValid)
                    {
                        return JsonSerializer.Serialize(new { 
                            isValid = false, 
                            message = $"The specialty '{appointment.Specialty}' is not available at our clinic. Please choose from our available specialties.",
                            appointment = appointment
                        });
                    }
                }

                // If no missing fields, appointment is valid
                if (missingFields.Count == 0 && specialtyValid)
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

        [KernelFunction, Description("Check available appointment slots for a specialty")]
        public async Task<string> CheckAvailability(
            [Description("The medical specialty to check availability for")]
            string specialty,
            [Description("The preferred date (optional)")]
            string date = "")
        {
            try
            {
                DateTime? fromDate = null;
                if (!string.IsNullOrEmpty(date))
                {
                    if (DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        fromDate = parsedDate;
                    }
                }

                var availableSlots = await _appointmentService.GetAvailableSlotsAsync(specialty, fromDate);
                
                if (availableSlots.Count == 0)
                {
                    return JsonSerializer.Serialize(new {
                        specialty = specialty,
                        date = date,
                        hasAvailability = false,
                        message = $"No available appointments found for {specialty}" + 
                                 (fromDate.HasValue ? $" on or after {fromDate.Value.ToString("MMMM d, yyyy")}" : ""),
                        availableSlots = new List<object>()
                    });
                }

                return JsonSerializer.Serialize(new {
                    specialty = specialty,
                    date = date,
                    hasAvailability = true,
                    message = $"Found {availableSlots.Count} available appointments for {specialty}",
                    availableSlots = availableSlots
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new {
                    specialty = specialty,
                    date = date,
                    hasAvailability = false,
                    message = $"Error checking availability: {ex.Message}",
                    availableSlots = new List<object>()
                });
            }
        }

        [KernelFunction, Description("Book an appointment and return confirmation details")]
        public async Task<string> BookAppointment(
            [Description("Patient name")]
            string patientName,
            [Description("Patient contact information (email or phone)")]
            string contactInfo,
            [Description("Appointment slot ID")]
            int slotId,
            [Description("Reason for the appointment")]
            string reason)
        {
            try
            {
                var request = new AppointmentRequest
                {
                    PatientName = patientName,
                    ContactInfo = contactInfo,
                    SlotId = slotId,
                    Reason = reason
                };

                var result = await _appointmentService.BookAppointmentAsync(request);
                
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new {
                    success = false,
                    message = $"Failed to book appointment: {ex.Message}"
                });
            }
        }

        [KernelFunction, Description("Get appointment details by confirmation code")]
        public async Task<string> GetAppointmentByCode(
            [Description("Confirmation code for the appointment")]
            string confirmationCode)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentByCodeAsync(confirmationCode);
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new {
                    success = false,
                    message = $"Failed to retrieve appointment: {ex.Message}"
                });
            }
        }
    }
}