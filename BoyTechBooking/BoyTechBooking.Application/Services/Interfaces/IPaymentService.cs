using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> HasPaidAsync(string userId);
        Task<bool> HasPaidAsync(string userId, int concertId); // concert-scoped check
        Task MarkPaidAsync(string userId);
        Task MarkPaidAsync(string userId, int concertId, decimal amount = 0m, string provider = "External", string externalPaymentId = null);

        // New: mark a single payment that covers multiple concerts.
        // concertAmounts maps concertId -> amount paid for that concert.
        Task MarkPaidAsync(string userId, decimal totalAmount, IDictionary<int, decimal> concertAmounts, string provider = "External", string externalPaymentId = null);
        // New: return total amount the user has paid for a given concert (sums direct payments + payment items)
        Task<decimal> GetTotalPaidForConcertAsync(string userId, int concertId);
    }
}
