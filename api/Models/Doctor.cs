using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebChatBot.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public int SpecialtyId { get; set; }
        
        [ForeignKey("SpecialtyId")]
        public virtual Specialty Specialty { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<TimeSlot> TimeSlots { get; set; }
    }
}