using BoyTechBooking.Application.Common;
using BoyTechBooking.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Infrastructure.Data
{
    public class DbInitial : IDbInitial
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitial(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Task Dataseed()
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult()) 
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                var admin = new ApplicationUser
                {
                    UserName = "boytechsolutions@gmail.com",
                    Email = "boytechsolutions@gmail.com"
                };
                _userManager.CreateAsync(admin, "Admin@12345").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
            }
            return Task.CompletedTask;
        }
    }
}
