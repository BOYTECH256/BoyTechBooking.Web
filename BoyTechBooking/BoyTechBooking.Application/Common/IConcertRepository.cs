using BoyTechBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Common
{
    public interface IConcertRepository :IGenericRepository<Concert>
    {
     void UpdateConcert(Concert concert);
    }
}
