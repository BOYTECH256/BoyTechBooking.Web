using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BoyTechBooking.Web.Models.ViewModels.ConcertViewModels
{
    public class EditConcertViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile? ChooseImage { get; set; }
        public DateTime Date { get; set; }
        public int VenueId { get; set; }
        public int ArtistId { get; set; }
    }
}
