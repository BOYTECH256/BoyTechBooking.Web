using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels
{
    public class BookSeatsViewModel
    {
        [Required]
        public int ConcertId { get; set; }

        public string? ConcertName { get; set; }

        // seats available to show on the form
        public List<int> AvailableSeats { get; set; } = new List<int>();

        // bound from form: one or more selected seat numbers
        public List<int> SelectedSeatNumbers { get; set; } = new List<int>();
    }
}
