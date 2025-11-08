using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        public required int SeatNumber { get; set; }
        [Required]
        public bool IsBooked { get; set; }
        public int? BookingId { get; set; }
        public Booking Booking { get; set; }

    }
}
