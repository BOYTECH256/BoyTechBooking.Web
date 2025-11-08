using BoyTechBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Interfaces
{
    public interface IArtistService
    {
        IEnumerable<Artist> GetAllArtist();
        Artist GetArtist(int id);
        Task SaveArtist(Artist artist);
        Task DeleteArtist(Artist artist);
        void UpdateArtist(Artist artist);

    }
}
