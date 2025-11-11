using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        // Returns true if there is at least one successful payment for the user (any concert).
        public async Task<bool> HasPaidAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var payments = _unitOfWork.PaymentRepository.GetAll(
                filter: p => p.ApplicationUserId == userId && p.IsSuccessful);

            if (payments != null && payments.Any()) return true;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (claims.Any(c => c.Type == "Paid" && c.Value == "true")) return true;
            }

            return false;
        }

        public Task<bool> HasPaidAsync(string userId, int concertId)
        {
            if (string.IsNullOrEmpty(userId) || concertId <= 0) return Task.FromResult(false);

            var payments = _unitOfWork.PaymentRepository.GetAll(
                filter: p => p.ApplicationUserId == userId && p.IsSuccessful,
                include: q => q.Include(p => p.Items));

            if (payments != null && payments.Any(p => (p.ConcertId.HasValue && p.ConcertId.Value == concertId)
                                                    || (p.Items != null && p.Items.Any(i => i.ConcertId == concertId))))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public async Task MarkPaidAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException(nameof(userId));

            var payment = new Payment
            {
                ApplicationUserId = userId,
                Amount = 0m,
                TotalAmount = 0m,
                PaidAt = DateTime.UtcNow,
                IsSuccessful = true,
                Provider = "Internal"
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            _unitOfWork.Save();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "Paid" && c.Value == "true"))
                {
                    await _userManager.AddClaimAsync(user, new Claim("Paid", "true"));
                }
            }
        }

        public async Task MarkPaidAsync(string userId, int concertId, decimal amount = 0m, string provider = "External", string externalPaymentId = null)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException(nameof(userId));
            if (concertId <= 0) throw new ArgumentException(nameof(concertId));

            var payment = new Payment
            {
                ApplicationUserId = userId,
                ConcertId = concertId,
                Amount = amount,
                TotalAmount = amount,
                PaidAt = DateTime.UtcNow,
                IsSuccessful = true,
                Provider = provider,
                ExternalPaymentId = externalPaymentId
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            _unitOfWork.Save();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "Paid" && c.Value == "true"))
                {
                    await _userManager.AddClaimAsync(user, new Claim("Paid", "true"));
                }
            }
        }

        public async Task MarkPaidAsync(string userId, decimal totalAmount, IDictionary<int, decimal> concertAmounts, string provider = "External", string externalPaymentId = null)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException(nameof(userId));
            if (totalAmount < 0) throw new ArgumentException(nameof(totalAmount));

            var payment = new Payment
            {
                ApplicationUserId = userId,
                ConcertId = null,
                Amount = totalAmount,
                TotalAmount = totalAmount,
                PaidAt = DateTime.UtcNow,
                IsSuccessful = true,
                Provider = provider,
                ExternalPaymentId = externalPaymentId
            };

            if (concertAmounts != null)
            {
                foreach (var kvp in concertAmounts)
                {
                    var item = new PaymentItem
                    {
                        ConcertId = kvp.Key,
                        Amount = kvp.Value
                    };
                    payment.Items.Add(item);
                }
            }

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            _unitOfWork.Save();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "Paid" && c.Value == "true"))
                {
                    await _userManager.AddClaimAsync(user, new Claim("Paid", "true"));
                }
            }
        }

        // New: total paid by user for a concert (sums payments where ConcertId==concertId + items)
        public async Task<decimal> GetTotalPaidForConcertAsync(string userId, int concertId)
        {
            if (string.IsNullOrEmpty(userId) || concertId <= 0) return 0m;

            var payments = _unitOfWork.PaymentRepository.GetAll(
                filter: p => p.ApplicationUserId == userId && p.IsSuccessful,
                include: q => q.Include(p => p.Items));

            if (payments == null) return 0m;

            decimal total = 0m;

            foreach (var p in payments)
            {
                if (p.ConcertId.HasValue && p.ConcertId.Value == concertId)
                {
                    total += p.Amount;
                }

                if (p.Items != null && p.Items.Any())
                {
                    total += p.Items.Where(i => i.ConcertId == concertId).Sum(i => i.Amount);
                }
            }

            return await Task.FromResult(total);
        }
    }
}
