using BoyTechBooking.Application.Common;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Infrastructure.Repository
{
    public class PaymentItemRepository : GenericRepository<PaymentItem>, IPaymentItemRepository
    {
        public PaymentItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
