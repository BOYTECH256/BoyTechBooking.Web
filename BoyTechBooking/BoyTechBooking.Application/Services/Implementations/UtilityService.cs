using BoyTechBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services.Implementations
{
    public class UtilityService : IUtilityService
    {
        private IWebHostEnvironment _env;
        private IHttpContextAccessor _contextAccessor;
        public UtilityService(IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
        {
            _env = env;
            _contextAccessor = contextAccessor;

        }

        public async Task<string> SaveImage(string containerName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty.", nameof(file));

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(_env.WebRootPath, containerName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}";
            var publicUrl = $"{baseUrl}/{containerName}/{fileName}";

            return publicUrl;
        }

        public async Task<string> EditImage(string containerName, IFormFile file, string dbpath)
        {
            await DeleteImage(containerName, dbpath);
            return await SaveImage(containerName, file);
        }

        public Task DeleteImage(string containerName, string dbpath)
        {
            if (string.IsNullOrWhiteSpace(dbpath))
                return Task.CompletedTask;

            var fileName = Path.GetFileName(dbpath);
            var completePath = Path.Combine(_env.WebRootPath, containerName, fileName);

            if (File.Exists(completePath))
                File.Delete(completePath);

            return Task.CompletedTask;
        }
    }
}