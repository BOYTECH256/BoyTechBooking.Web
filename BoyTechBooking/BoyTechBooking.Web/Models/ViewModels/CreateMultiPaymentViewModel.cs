using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Web.Models.ViewModels
{
    public class CreateMultiPaymentViewModel
    {
        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        // each entry represents a (concertId, amount) pair
        public List<ConcertPaymentItemViewModel> Items { get; set; } = new List<ConcertPaymentItemViewModel>();

        // helper for the view (concert id -> name)
        public List<int> AvailableConcertIds { get; set; } = new List<int>();
    }

    public class ConcertPaymentItemViewModel
    {
        public int ConcertId { get; set; }
        public string ConcertName { get; set; }
        [Range(0.00, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}

