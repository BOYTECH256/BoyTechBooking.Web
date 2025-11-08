using Microsoft.AspNetCore.Mvc;

namespace BoyTechBooking.Web.Controllers
{
    public class BookingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
