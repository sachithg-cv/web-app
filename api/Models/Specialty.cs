using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebChatBot.Models
{
    public class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}