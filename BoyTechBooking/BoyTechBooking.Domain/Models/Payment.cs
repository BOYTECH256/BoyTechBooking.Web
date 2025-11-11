using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        // Optional: tie payment to a specific concert (nullable)
        public int? ConcertId { get; set; }

        // Amount for this payment record (useful for single-concert payments)
        [Required]
        public decimal Amount { get; set; }

        // Total amount (useful when a single payment covers multiple concerts)
        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Simple success flag, extend to enum/status as needed
        public bool IsSuccessful { get; set; } = true;

        // Provider / external payment id for reconciliation
        public string? Provider { get; set; }
        public string? ExternalPaymentId { get; set; }

        // If this payment covers multiple concerts, list the covered items
        public ICollection<PaymentItem> Items { get; set; } = new List<PaymentItem>();
    }
}
