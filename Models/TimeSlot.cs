using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebChatBot.Models
{
    public class TimeSlot
    {
        [Key]
        public int SlotId { get; set; }
        
        public int DoctorId { get; set; }
        
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
        
        private DateTime _startTime;
        
        [Required]
        public DateTime StartTime 
        { 
            get => _startTime;
            set => _startTime = value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
                : (value.Kind == DateTimeKind.Local 
                    ? value.ToUniversalTime() 
                    : value);
        }
        
        private DateTime _endTime;
        
        [Required]
        public DateTime EndTime 
        { 
            get => _endTime;
            set => _endTime = value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
                : (value.Kind == DateTimeKind.Local 
                    ? value.ToUniversalTime() 
                    : value);
        }
        
        public bool IsAvailable { get; set; } = true;
        
        public virtual Appointment Appointment { get; set; }
    }
}