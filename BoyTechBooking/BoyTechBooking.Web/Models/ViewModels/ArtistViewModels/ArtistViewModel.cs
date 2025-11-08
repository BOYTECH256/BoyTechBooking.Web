using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels.ArtistViewModels
{
    public class ArtistViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }

    }
}
