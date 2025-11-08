using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels.ArtistViewModels
{
    public class EditArtistViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ChooseImage { get; set; }
    }
}
