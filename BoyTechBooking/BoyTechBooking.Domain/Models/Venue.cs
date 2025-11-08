using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int SeatCapacity { get; set; }
        public ICollection<Concert> Concerts { get; set; } = new List<Concert>();


    }
}
