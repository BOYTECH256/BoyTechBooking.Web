using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddBooking(Booking booking)
        {
            ArgumentNullException.ThrowIfNull(booking);
            await _unitOfWork.BookingRepository.AddAsync(booking);
            _unitOfWork.Save();
        }

        public IEnumerable<Booking> GetAllBooking(int concertId)
        {
            var bookings = _unitOfWork.BookingRepository.GetAll(
                filter: x => x.ConcertId == concertId,
                include: b => b
                    .Include(c => c.Concert)
                    .Include(u => u.ApplicationUser));
                    return bookings;            
        }
    }
}
