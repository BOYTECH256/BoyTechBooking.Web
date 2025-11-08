using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels.ConcertViewModels
{
    public class ConcertViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ArtistName { get; set; }
        public string VenueName { get; set; }
    }
}
