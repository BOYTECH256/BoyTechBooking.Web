using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Implementations
{
    public class ArtistService : IArtistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArtistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteArtist(Artist artist)
        {
            ArgumentNullException.ThrowIfNull(artist);
            await _unitOfWork.ArtistRepository.DeleteAsync(artist);
            _unitOfWork.Save();
        }

        public IEnumerable<Artist> GetAllArtist()
        {
            return _unitOfWork.ArtistRepository.GetAll();
        }

        public Artist GetArtist(int id)
        {
            var artist = _unitOfWork.ArtistRepository.GetById(id);
            if (artist == null)
            {
                throw new Exception("Artist null");
            }
            return artist;
        }

        public async Task SaveArtist(Artist artist)
        {
            ArgumentNullException.ThrowIfNull(artist);
            await _unitOfWork.ArtistRepository.AddAsync(artist);
            _unitOfWork.Save();
        }

        public void UpdateArtist(Artist artist)
        {
            ArgumentNullException.ThrowIfNull(artist);
            _unitOfWork.ArtistRepository.UpdateArtist(artist);
            _unitOfWork.Save();
        }
    }
}
