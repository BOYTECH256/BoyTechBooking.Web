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
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> BookTicketsAsync(int concertId, List<int> seatNumbers, string userId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetBookedTickets(int concertId)
        {
            var bookedSeatNumber = _unitOfWork.TicketRepository
                .GetAll(include: x => x.Include(b=>b.Booking))
                .Where(x => x.Booking.ConcertId == concertId && x.IsBooked)
                .Select(t => t.SeatNumber)
                .ToList();
            return bookedSeatNumber;
        }

        public IEnumerable<Booking> GetBookings(string userId)
        {
            var bookings = _unitOfWork.BookingRepository
                .GetAll(include: x => x.Include(a => a.Concert).Include(t => t.Tickets))
                .Where(x => x.ApplicationUserId == userId)
                .ToList();
            return bookings;

        }
    }
}
