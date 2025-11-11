using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Services.Implementations;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace BoyTechBooking.Web.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ITicketService _ticketService;
        private readonly IConcertService _concertService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;
        public BookingsController(IBookingService bookingService, ITicketService ticketService, IConcertService concertService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _ticketService = ticketService;
            _concertService = concertService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        // Admin: list bookings for a concert
        //[Authorize(Roles = "Admin")]
        public IActionResult Index(int concertId)
        {
            if (concertId <= 0) return BadRequest();

            var bookings = _bookingService.GetAllBooking(concertId);

            var vm = bookings.Select(b => new BookingViewModel
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                ConcertName = b.Concert?.Name ?? string.Empty,
                Tickets = b.Tickets.Select(t => new TicketViewModel { SeatNumber = t.SeatNumber }).ToList()
            }).ToList();
            return View(vm);
        }

        // Details: admin or owner can view
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(
                b => b.BookingId == id,
                include: q => q.Include(b => b.Concert)
                              .Include(b => b.Tickets)
                              .Include(b => b.ApplicationUser));

            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && booking.ApplicationUserId != userId) return Forbid();

            var vm = new BookingViewModel
            {
                BookingId = booking.BookingId,
                BookingDate = booking.BookingDate,
                ConcertName = booking.Concert?.Name ?? string.Empty,
                Tickets = booking.Tickets.Select(t => new TicketViewModel { SeatNumber = t.SeatNumber }).ToList()
            };

            return View(vm);
        }

        // Admin: GET create booking page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var concerts = _concertService.GetAllConcert().ToList();
            ViewBag.Concerts = new SelectList(concerts, "Id", "Name");

            var users = _userManager.Users.ToList();
            ViewBag.Users = new SelectList(users, "Id", "UserName");

            return View();
        }

        // Admin: POST create booking (either via ticket service if seats provided, or simple booking)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var concerts = _concertService.GetAllConcert().ToList();
                ViewBag.Concerts = new SelectList(concerts, "Id", "Name");
                var users = _userManager.Users.ToList();
                ViewBag.Users = new SelectList(users, "Id", "UserName");
                return View(vm);
            }

            if (vm.SeatNumbers != null && vm.SeatNumbers.Any())
            {
                // prefer ticket service which creates booking + tickets
                var booked = await _ticketService.BookTicketsAsync(vm.ConcertId, vm.SeatNumbers, vm.ApplicationUserId);
                if (!booked)
                {
                    ModelState.AddModelError(string.Empty, "Failed to book tickets (some seats may be unavailable).");
                    var concerts = _concertService.GetAllConcert().ToList();
                    ViewBag.Concerts = new SelectList(concerts, "Id", "Name");
                    var users = _userManager.Users.ToList();
                    ViewBag.Users = new SelectList(users, "Id", "UserName");
                    return View(vm);
                }

                return RedirectToAction(nameof(Index), new { concertId = vm.ConcertId });
            }

            // create empty booking (no tickets)
            var booking = new Booking
            {
                BookingDate = vm.BookingDate == default ? DateTime.UtcNow : vm.BookingDate,
                ConcertId = vm.ConcertId,
                ApplicationUserId = vm.ApplicationUserId
            };

            await _bookingService.AddBooking(booking);

            return RedirectToAction(nameof(Index), new { concertId = vm.ConcertId });
        }

        // Admin: delete booking
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(
                b => b.BookingId == id,
                include: q => q.Include(b => b.Tickets));

            if (booking != null)
            {
                // remove related tickets first if repository requires it (Delete cascades may exist)
                if (booking.Tickets?.Any() == true)
                {
                    _unitOfWork.TicketRepository.DeleteRange(booking.Tickets.ToList());
                }

                _unitOfWork.BookingRepository.Delete(booking);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index), new { concertId = booking.ConcertId });
            }

            return NotFound();
        }

        // End-user: view their bookings
        [Authorize]
        public IActionResult MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = _ticketService.GetBookings(userId);
            var vm = bookings.Select(booking => new BookingViewModel
            {
                BookingId = booking.BookingId,
                BookingDate = booking.BookingDate,
                ConcertName = booking.Concert?.Name ?? string.Empty,
                Tickets = booking.Tickets.Select(t => new TicketViewModel { SeatNumber = t.SeatNumber }).ToList()
            }).ToList();

            return View(vm);
        }

        // End-user: GET seat selection / booking page
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Book(int concertId)
        {
            if (concertId <= 0) return BadRequest();

            var concert = await _concertService.GetConcert(concertId);
            if (concert == null) return NotFound();

            var allSeats = Enumerable.Range(1, concert.Venue.SeatCapacity).ToList();
            var bookedSeats = _ticketService.GetBookedTickets(concertId)?.ToList() ?? new List<int>();
            var availableSeats = allSeats.Except(bookedSeats).ToList();

            var vm = new BookSeatsViewModel
            {
                ConcertId = concertId,
                ConcertName = concert.Name,
                AvailableSeats = availableSeats
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookSeatsViewModel vm)
        {
            if (!ModelState.IsValid) return await RebuildBookView(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            // concert-scoped payment check
            var hasPaidForConcert = await _paymentService.HasPaidAsync(userId, vm.ConcertId);
            if (!hasPaidForConcert)
            {
                // pass concert info to the payment required view
                return View("PaymentRequired", vm);
            }

            if (vm.SelectedSeatNumbers == null || !vm.SelectedSeatNumbers.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one seat.");
                return await RebuildBookView(vm);
            }

            var booked = await _ticketService.BookTicketsAsync(vm.ConcertId, vm.SelectedSeatNumbers, userId);
            if (!booked)
            {
                ModelState.AddModelError(string.Empty, "Failed to book tickets — some selected seats are no longer available.");
                return await RebuildBookView(vm);
            }

            return RedirectToAction(nameof(MyBookings));
        }
        private async Task<IActionResult> RebuildBookView(BookSeatsViewModel vm)
        {
            var concert = await _concertService.GetConcert(vm.ConcertId);
            if (concert == null) return NotFound();

            var allSeats = Enumerable.Range(1, concert.Venue.SeatCapacity).ToList();
            var bookedSeats = _ticketService.GetBookedTickets(vm.ConcertId)?.ToList() ?? new List<int>();
            vm.AvailableSeats = allSeats.Except(bookedSeats).ToList();
            vm.ConcertName = concert.Name;
            return View("Book", vm);
        }

        // simple page if the user hasn't paid
        [Authorize]
        public IActionResult PaymentRequired()
        {
            return View();
        }
    }
}
