using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        [ForeignKey("ConcertId")]
        public int ConcertId { get; set; }
        public Concert Concert { get; set; }
        [Required]
        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; } 
       public ApplicationUser ApplicationUser { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
