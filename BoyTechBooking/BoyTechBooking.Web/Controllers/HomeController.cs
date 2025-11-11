using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Services.Implementations;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models;
using BoyTechBooking.Web.Models.ViewModels.DashboardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        private readonly IConcertService _concertService;
        private readonly IBookingService _bookingService;
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;
        public HomeController(ILogger<HomeController> logger, IConcertService concertService,
            IBookingService bookingService, ITicketService ticketService,
            IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IPaymentService paymentService)
        {
            _logger = logger;
            _concertService = concertService;
            _bookingService = bookingService;
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            DateTime today = DateTime.Today;
            var concerts = _concertService.GetAllConcert();
            var vm = concerts.Where(d => d.Date.Date >= today)
                .Select(x => new HomeConcertViewModel
            {
                ConcertId = x.Id,
                ConcertName = x.Name,
                ArtisttName = x.Artist.Name,
                ConcertImage = x.ImageUrl,
                Description = x.Description.Length>100 ? x.Description.Substring(0, 100) + "..." : x.Description
                }).ToList();
            return View(vm);
        }
        
        [Authorize]
        public async Task<IActionResult> AvailableTickets(int id)
        {
            var concert = await _concertService.GetConcert(id);
            if (concert == null)
            {
                return NotFound();
            }
            var allseats = Enumerable.Range(1, concert.Venue.SeatCapacity).ToList();
            var bookedTickets = _ticketService.GetBookedTickets(concert.Id);
            var availableSeats = allseats.Except(bookedTickets).ToList();
            var viewModel = new AvailableTicketViewModel
            {
                ConcertId = concert.Id,
                ConcertName = concert.Name,
                AvailableSeats = availableSeats
            };
            return View(viewModel);
        }
        public async Task<bool> BookTicketsAsync(int concertId, List<int> seatNumbers, string userId)
        {
            if (seatNumbers == null || !seatNumbers.Any())
                throw new ArgumentException("No seats selected for booking.");

            // Verify concert exists
            var concert = await _unitOfWork.ConcertRepository
                .GetByIdAsync(x => x.Id == concertId, include: x => x.Include(c => c.Venue));

            if (concert == null)
                throw new Exception($"Concert with ID {concertId} not found.");

            // Get all currently booked seats to prevent double-booking
            var alreadyBooked = _unitOfWork.TicketRepository
                .GetAll(include: x => x.Include(b => b.Booking))
                .Where(x => x.Booking.ConcertId == concertId && x.IsBooked)
                .Select(x => x.SeatNumber)
                .ToList();

            var conflictSeats = seatNumbers.Intersect(alreadyBooked).ToList();
            if (conflictSeats.Any())
                throw new Exception($"Seats already booked: {string.Join(", ", conflictSeats)}");

            // Create booking record
            var newBooking = new Booking
            {
                ConcertId = concertId,
                ApplicationUserId = userId,
                BookingDate = DateTime.UtcNow,
                Tickets = seatNumbers.Select(seat => new Ticket
                {
                    SeatNumber = seat,
                    IsBooked = true
                }).ToList()
            };

            await _unitOfWork.BookingRepository.AddAsync(newBooking);
            _unitOfWork.Save();

            return true;
        }
        [HttpPost]
        public IActionResult BookTickets(int concertId, List<int> selectedSeats)
        {
            if (selectedSeats == null || selectedSeats.Count==0)
            {
                TempData["Error"] = "Please select at least one seat.";
                return RedirectToAction("AvailableTickets", new { id = concertId });
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userid = claim.Value;
            var booking = new Booking
            {
                ConcertId= concertId,
                BookingDate =DateTime.Now,
                ApplicationUserId =userid
            };
               foreach (var seatNumber in selectedSeats) 
            {
                booking.Tickets.Add(new Ticket
                { 
                    SeatNumber = seatNumber,
                    IsBooked = true
                });
                _bookingService.AddBooking(booking);
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = _ticketService.GetBookings(userId);
            return View(bookings);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var concert = await _concertService.GetConcert(id);
            if (concert == null)
            {
                return NotFound();
            }
            var vm = new HomeConcertDetailsViewModel
            {
                ConcertId = concert.Id,
                ConcertName = concert.Name,
                ConcertDateTime = concert.Date,
                Description = concert.Description,
                ArtistName = concert.Artist.Name,
                ArtistImage = concert.Artist.ImageUrl,
                VenueName = concert.Venue.Name,
                VenueAddress = concert.Venue.Address,
                ConcertImage = concert.ImageUrl
            };
            // show per-concert paid state and total for the current user
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                vm.HasPaidForThisConcert = await _paymentService.HasPaidAsync(userId, concert.Id);
                vm.TotalPaidForThisConcert = await _paymentService.GetTotalPaidForConcertAsync(userId, concert.Id);
            }

            return View(vm);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
