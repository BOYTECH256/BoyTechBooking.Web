using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoyTechBooking.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VenuesController : Controller
    {
        private IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        public IActionResult Index()
        {
            List<VenueViewModel>vm =new List<VenueViewModel>();
            var venues = _venueService.GetAllVenue();
            foreach (var item in venues)
            {
                vm.Add(new VenueViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Address = item.Address,
                    SeatCapacity = item.SeatCapacity
                });
            }
            return View(vm);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CreateVenueViewModel vm)
        {
            var venue = new Venue
            {
                Name = vm.Name,
                Address = vm.Address,
                SeatCapacity = vm.SeatCapacity
            };
            _venueService.SaveVenue(venue);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var venue = _venueService.GetVenue(id);
            var vm = new CreateVenueViewModel
            {
                Id=venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                SeatCapacity = venue.SeatCapacity
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult Edit(CreateVenueViewModel vm)
        {
            var venue=_venueService.GetVenue(vm.Id);
            _venueService.UpdateVenue(venue);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var venue = _venueService.GetVenue(id);
            _venueService.DeleteVenue(venue);
            return RedirectToAction("Index");
        }
    }
}
