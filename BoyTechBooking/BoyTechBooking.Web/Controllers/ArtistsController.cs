using BoyTechBooking.Application.Services.Implementations;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Web.Models.ViewModels;
using BoyTechBooking.Web.Models.ViewModels.ArtistViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class ArtistsController : Controller
    {
        private readonly IUtilityService _utilityService;
        private readonly IArtistService _artistService;
        private string ContainerName = "ArtistImage";

        public ArtistsController(IUtilityService utilityService, IArtistService artistService)
        {
            _utilityService = utilityService;
            _artistService = artistService;
        }     
        public IActionResult Index()
        {
            List<ArtistViewModel> vm = new List<ArtistViewModel>();
            var artists = _artistService.GetAllArtist();
            foreach (var artist in artists)
            {
                vm.Add(new ArtistViewModel
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Bio = artist.Bio,
                    ImageUrl = artist.ImageUrl
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
        public async Task <IActionResult> Create(CreateArtistViewModel vm)
        {
            var artist = new Artist
            {
                Name = vm.Name,
                Bio = vm.Bio
            };
            if (vm.ImageUrl != null)
            {
                artist.ImageUrl = await _utilityService.SaveImage(ContainerName,vm.ImageUrl);
            }
           await _artistService.SaveArtist(artist);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var artist = _artistService.GetArtist(id);
            var vm = new EditArtistViewModel
            {
                Id = artist.Id,
                Name = artist.Name,
                Bio = artist.Bio,
                ImageUrl = artist.ImageUrl
            };
            return View(vm);
        }
        [HttpPost]
        public async Task <IActionResult> Edit(EditArtistViewModel vm)
        {
            var artist = _artistService.GetArtist(vm.Id);
            artist.Name = vm.Name;
            artist.Bio = vm.Bio;
            if (vm.ChooseImage != null)
            {
                artist.ImageUrl = await _utilityService.EditImage(
                    ContainerName, vm.ChooseImage, artist.ImageUrl);
            }
            _artistService.UpdateArtist(artist);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = _artistService.GetArtist(id);
            await _utilityService.DeleteImage(ContainerName, artist.ImageUrl);
           await _artistService.DeleteArtist(artist);
            return RedirectToAction("Index");
        }
    }
}
