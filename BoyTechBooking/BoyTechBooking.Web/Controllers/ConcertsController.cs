using BoyTechBooking.Application.Services.Implementations;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models.ViewModels.ArtistViewModels;
using BoyTechBooking.Web.Models.ViewModels.ConcertViewModels;
using BoyTechBooking.Web.Models.ViewModels.DashboardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace BoyTechBooking.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ConcertsController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IBookingService _bookingService;
        private readonly IConcertService _concertService;
        private readonly IUtilityService _utilityService;
        private readonly IVenueService _venueService;
        private readonly IPaymentService _paymentService;
        private string ContainerName = "ConcertImage";

        public ConcertsController(IArtistService artistService,
            IBookingService bookingService,
            IConcertService concertService,
            IUtilityService utilityService,
            IVenueService venueService,
            IPaymentService paymentService)
        {
            _artistService = artistService;
            _bookingService = bookingService;
            _concertService = concertService;
            _utilityService = utilityService;
            _venueService = venueService;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            var concerts = _concertService.GetAllConcert();
            List<ConcertViewModel> vm = new List<ConcertViewModel>();
            foreach (var item in concerts)
            {
                vm.Add(new ConcertViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    ArtistName = item.Artist.Name,
                    VenueName = item.Venue.Name,
                    Date = item.Date
                });
            }
            return View(vm);
        }
        //Get: Concerts/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var venues = _venueService.GetAllVenue();
            var artists = _artistService.GetAllArtist();
            ViewBag.Venues = new SelectList(venues, "Id", "Name");
            ViewBag.Artists = new SelectList(artists, "Id", "Name");
            return View();
        }
        //Post: Concerts/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateConcertViewModel vm)
        {
            var concert = new Concert
            {
                Name = vm.Name,
                Description = vm.Description,
                Date = vm.Date,
                VenueId = vm.VenueId,
                ArtistId = vm.ArtistId
            };
            if (vm.ImageUrl != null)
            {
                concert.ImageUrl = await _utilityService.SaveImage(ContainerName, vm.ImageUrl);
            }
            await _concertService.SaveConcert(concert);
            return RedirectToAction("Index");
        }
        //Get: Concerts/Edit
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task <IActionResult> Edit(int id)
        {
            var concert = await _concertService.GetConcert(id);
            var artists = _artistService.GetAllArtist();
            var venues = _venueService.GetAllVenue();
            ViewBag.Artist = new SelectList(artists, "Id", "Name" );
            ViewBag.VenueList = new SelectList(venues, "Id", "Name");
            var vm = new EditConcertViewModel
            {
                Id = concert.Id,
                Name = concert.Name,
                Description = concert.Description,
                ImageUrl = concert.ImageUrl,
                Date = concert.Date,
                VenueId = concert.VenueId,
                ArtistId = concert.ArtistId
            };
            return View(vm);

        }
        //Post: Concerts/Edit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditConcertViewModel vm)
        {
            var concert = await _concertService.GetConcert(vm.Id);
            concert.Id = vm.Id;
            concert.Description = vm.Description;
            concert.Name = vm.Name;
            concert.Date = vm.Date;
            concert.ArtistId = vm.ArtistId;
            concert.VenueId = vm.VenueId;
            if (vm.ChooseImage != null)
            {
                concert.ImageUrl = await _utilityService.EditImage(
                    ContainerName, vm.ChooseImage, concert.ImageUrl);
            }
            _concertService.UpdateConcert(concert);
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var concert =await _concertService.GetConcert(id);
            if (concert != null) 
            {
                await _utilityService.DeleteImage(ContainerName, concert.ImageUrl);
                await _concertService.DeleteConcert(concert);

            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTickets(int id)
        { 
            var booking =  _bookingService.GetAllBooking(id);
            var vm =booking.Select(b => new DashboardViewModel
            {
                UserName = b.ApplicationUser.UserName,
                ConcertName = b.Concert.Name,
                SeatNumber = string.Join(", ", b.Tickets.Select(t=>t.SeatNumber))
            }).ToList();            
            return View(vm);
        }
        // Details view for all users; shows payment state and enables booking link when paid
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var concert = await _concertService.GetConcert(id);
            if (concert == null) return NotFound();

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

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                vm.HasPaidForThisConcert = await _paymentService.HasPaidAsync(userId, concert.Id);
                vm.TotalPaidForThisConcert = await _paymentService.GetTotalPaidForConcertAsync(userId, concert.Id);
            }

            return View(vm);
        }

    }
}
