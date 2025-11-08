using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class Concert
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [ForeignKey("VenueId")]
        public int VenueId { get; set; }
        public Venue Venue { get; set; }
        [Required]
        [ForeignKey("ArtistId")]
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
