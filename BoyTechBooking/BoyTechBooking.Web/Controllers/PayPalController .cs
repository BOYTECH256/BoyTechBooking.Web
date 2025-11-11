using BoyTechBooking.Application.Services;
using Microsoft.AspNetCore.Mvc;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
namespace BoyTechBooking.Web.Controllers
{
    [Route("paypal")]
    public class PayPalController : Controller
    {
        private readonly PayPalClientFactory _factory;
        private readonly ILogger<PayPalController> _logger;
        public PayPalController(PayPalClientFactory factory, ILogger<PayPalController> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Displays the PayPal checkout start page with a payment form.
        /// </summary>
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            _logger.LogInformation("PayPal checkout page accessed.");
            return View("Index");
        }

        /// <summary>
        /// Creates a PayPal order and redirects the user to the PayPal-hosted checkout page.
        /// </summary>
        [HttpPost("create-order")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder([FromForm] decimal amount, [FromForm] string currency = "USD")
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount: {Amount}", amount);
                return BadRequest("Invalid payment amount.");
            }

            try
            {
                var client = _factory.CreateClient();

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");

                request.RequestBody(new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>
                    {
                        new PurchaseUnitRequest
                        {
                            AmountWithBreakdown = new AmountWithBreakdown
                            {
                                CurrencyCode = currency,
                                Value = amount.ToString("F2")
                            }
                        }
                    },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = Url.Action("Success", "PayPal", null, Request.Scheme),
                        CancelUrl = Url.Action("Cancel", "PayPal", null, Request.Scheme),
                        BrandName = "BoyTech Booking Services",
                        UserAction = "PAY_NOW"
                    }
                });

                PayPalHttp.HttpResponse response = await client.Execute(request);
                var result = response.Result<Order>();

                var approveLink = result.Links.FirstOrDefault(l => l.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;

                if (string.IsNullOrEmpty(approveLink))
                {
                    _logger.LogError("PayPal approval link not found in response.");
                    return StatusCode(500, "Unable to create PayPal order. Please try again later.");
                }

                _logger.LogInformation("Redirecting user to PayPal approval URL: {Url}", approveLink);
                return Redirect(approveLink);
            }
            catch (HttpException httpEx)
            {
                _logger.LogError(httpEx, "PayPal API error: {Message}", httpEx.Message);
                return StatusCode(500, "PayPal service unavailable. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during PayPal order creation.");
                return StatusCode(500, "Unexpected error. Please try again later.");
            }
        }

        /// <summary>
        /// Handles successful payment completion after redirect from PayPal.
        /// </summary>
        [HttpGet("success")]
        public async Task<IActionResult> Success(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Missing PayPal token on success redirect.");
                return BadRequest("Missing PayPal payment token.");
            }

            try
            {
                var client = _factory.CreateClient();
                var captureRequest = new OrdersCaptureRequest(token);
                captureRequest.RequestBody(new OrderActionRequest());

                var response = await client.Execute(captureRequest);
                var result = response.Result<Order>();

                _logger.LogInformation("PayPal payment capture completed: Status={Status}, ID={Id}", result.Status, result.Id);

                if (result.Status?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // TODO: Mark booking/order as paid in your database
                    // await _bookingService.MarkPaidAsync(result.Id);

                    return View("Success");
                }

                // Payment pending (manual review or delayed capture)
                return RedirectToAction("Pending");
            }
            catch (HttpException httpEx)
            {
                _logger.LogError(httpEx, "PayPal capture failed: {Message}", httpEx.Message);
                return StatusCode(500, "Payment could not be verified. Please contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error capturing PayPal payment.");
                return StatusCode(500, "Unexpected error verifying payment.");
            }
        }

        /// <summary>
        /// Displays a pending payment message when PayPal status isn't completed.
        /// </summary>
        [HttpGet("pending")]
        public IActionResult Pending()
        {
            _logger.LogInformation("Payment pending view displayed.");
            return View("Pending");
        }

        /// <summary>
        /// Called when the user cancels the PayPal checkout process.
        /// </summary>
        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            _logger.LogInformation("PayPal payment cancelled by user.");
            return View("Cancel");
        }
    }
}
