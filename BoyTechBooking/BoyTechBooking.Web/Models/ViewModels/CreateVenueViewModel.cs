using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels
{
    public class CreateVenueViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int SeatCapacity { get; set; }
    }
}
