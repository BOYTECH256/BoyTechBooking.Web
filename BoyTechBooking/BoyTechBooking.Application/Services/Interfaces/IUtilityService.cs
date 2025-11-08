using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Interfaces
{
    public interface IUtilityService
    {
        Task<string> SaveImage(string containerName, IFormFile file);
        Task<string> EditImage(string containerName, IFormFile file, string dbpath);
        Task DeleteImage(string containerName, string dbpath);

    }
}
