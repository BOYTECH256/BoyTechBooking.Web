using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Domain.Models
{
    public class ApplicationUser:IdentityUser
    {
      public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
