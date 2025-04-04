using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebChatBot.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        
        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        
        public int SlotId { get; set; }
        
        [ForeignKey("SlotId")]
        public virtual TimeSlot Slot { get; set; }
        
        public string Reason { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "confirmed";
        
        [StringLength(20)]
        public string ConfirmationCode { get; set; }
        
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    }
}