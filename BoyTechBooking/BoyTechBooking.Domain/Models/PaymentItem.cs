using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class PaymentItem
    {
        [Key]
        public int PaymentItemId { get; set; }

        [Required]
        [ForeignKey("PaymentId")]
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        [Required]
        public int ConcertId { get; set; }

        // Amount paid for this specific concert
        [Required]
        public decimal Amount { get; set; }
    }
}
