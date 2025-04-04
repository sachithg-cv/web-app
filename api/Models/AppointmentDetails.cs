using System;
using System.Text.Json.Serialization;

namespace WebChatBot.Models
{
    public class AppointmentDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("patientName")]
        public string PatientName { get; set; } = "unknown";
        
        [JsonPropertyName("specialty")]
        public string Specialty { get; set; } = "unknown";
        
        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; } = "unknown";
        
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "unknown";
        
        [JsonPropertyName("contactInfo")]
        public string ContactInfo { get; set; } = "unknown";
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";
        
        [JsonPropertyName("confirmationTime")]
        public DateTime? ConfirmationTime { get; set; }
        
        [JsonIgnore]
        public bool IsComplete => 
            PatientName != "unknown" &&
            Specialty != "unknown" &&
            DateTime != "unknown" &&
            Reason != "unknown" &&
            ContactInfo != "unknown";
        
        [JsonIgnore]
        public string Summary => 
            $"Patient: {PatientName}\n" +
            $"Specialty: {Specialty}\n" +
            $"Date/Time: {DateTime}\n" +
            $"Reason: {Reason}\n" +
            $"Contact: {ContactInfo}\n" +
            $"Status: {Status}";
            
        // If you still need methods for backward compatibility
        public bool IsCompleteCheck()
        {
            return IsComplete;
        }
        
        public string GetSummary()
        {
            return Summary;
        }
    }
}