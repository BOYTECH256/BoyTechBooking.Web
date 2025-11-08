using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models.ViewModels.ArtistViewModels;
using BoyTechBooking.Web.Models.ViewModels.ConcertViewModels;
using BoyTechBooking.Web.Models.ViewModels.DashboardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoyTechBooking.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConcertsController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IBookingService _bookingService;
        private readonly IConcertService _concertService;
        private readonly IUtilityService _utilityService;
        private readonly IVenueService _venueService;
        private string ContainerName = "ConcertImage";

        public ConcertsController(IArtistService artistService, 
            IBookingService bookingService, 
            IConcertService concertService,
            IUtilityService utilityService, 
            IVenueService venueService)
        {
            _artistService = artistService;
            _bookingService = bookingService;
            _concertService = concertService;
            _utilityService = utilityService;
            _venueService = venueService;
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

    }
}
