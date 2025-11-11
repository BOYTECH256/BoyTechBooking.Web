using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels
{
    public class CreateBookingViewModel
    {
        [Required]
        public int ConcertId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        // optional — if provided the controller will attempt to book these seats via ITicketService
        public List<int> SeatNumbers { get; set; } = new List<int>();

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    }
}
