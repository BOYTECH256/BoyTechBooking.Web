using BoyTechBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Interfaces
{
    public interface IConcertService
    {
        IEnumerable<Concert> GetAllConcert();
        Task<Concert> GetConcert(int id);
        Task SaveConcert(Concert concert);
        Task DeleteConcert(Concert concert);
        void UpdateConcert(Concert concert);

    }
}
