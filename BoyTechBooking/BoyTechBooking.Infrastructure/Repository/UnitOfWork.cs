using BoyTechBooking.Application.Common;
using BoyTechBooking.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context )
        {
            _context = context;
            ArtistRepository = new ArtistRepository(_context);
            BookingRepository = new BookingRepository(_context);
            ConcertRepository = new ConcertRepository (_context);
            TicketRepository = new TicketRepository(_context);
            VenueRepository = new VenueRepository(_context);
        }
        public IArtistRepository ArtistRepository { get; private set; }
        public IBookingRepository BookingRepository { get; private set; }
        public IConcertRepository ConcertRepository { get; private set; }
        public ITicketRepository TicketRepository { get; private set; }
        public IVenueRepository VenueRepository { get; private set; }
        public void Save()
        {
            _context.SaveChanges();
        }

    }
}
