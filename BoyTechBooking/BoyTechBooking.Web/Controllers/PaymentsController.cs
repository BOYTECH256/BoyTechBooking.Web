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

namespace BoyTechBooking.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class PaymentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IConcertService _concertService;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentsController(IUnitOfWork unitOfWork, IPaymentService paymentService, IConcertService concertService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _concertService = concertService;
            _userManager = userManager;
        }

        // GET: Payments
        public IActionResult Index()
        {
            var payments = _unitOfWork.PaymentRepository.GetAll(
                include: q => q.Include(p => p.ApplicationUser).Include(p => p.Items));
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(
                p => p.PaymentId == id,
                include: q => q.Include(x => x.ApplicationUser).Include(x => x.Items));
            if (payment == null) return NotFound();
            return View(payment);
        }

        // GET: Payments/Create (multi-concert)
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            var concerts = _concertService.GetAllConcert().ToList();
            var users = _userManager.Users.ToList();

            var vm = new CreateMultiPaymentViewModel
            {
                AvailableConcertIds = concerts.Select(c => c.Id).ToList(),
                Items = concerts.Select(c => new ConcertPaymentItemViewModel
                {
                    ConcertId = c.Id,
                    ConcertName = c.Name,
                    Amount = 0m
                }).ToList()
            };

            ViewBag.Users = new SelectList(users, "Id", "UserName");
            return View(vm);
        }

        // POST: Payments/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMultiPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var concerts = _concertService.GetAllConcert().ToList();
                var users = _userManager.Users.ToList();
                ViewBag.Users = new SelectList(users, "Id", "UserName");
                return View(vm);
            }

            // build dictionary of concert -> amount for non-zero amounts
            var concertAmounts = vm.Items?.Where(i => i.Amount > 0).ToDictionary(i => i.ConcertId, i => i.Amount);

            await _paymentService.MarkPaidAsync(vm.ApplicationUserId, vm.TotalAmount, concertAmounts, provider: "AdminCreated", externalPaymentId: null);

            return RedirectToAction(nameof(Index));
        }

        // POST: Payments/Reconcile/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reconcile(int id, bool markSuccessful = true)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(p => p.PaymentId == id);
            if (payment == null) return NotFound();

            payment.IsSuccessful = markSuccessful;

            // use repository Update to persist change
            _unitOfWork.PaymentRepository.Update(payment);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
