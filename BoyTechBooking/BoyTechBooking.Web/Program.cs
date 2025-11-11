using BoyTechBooking.Application.Common;
using BoyTechBooking.Application.Common.Utility;
using BoyTechBooking.Application.Services;
using BoyTechBooking.Application.Services.Implementations;
using BoyTechBooking.Application.Services.Interfaces;
using BoyTechBooking.Domain.Models;
using BoyTechBooking.Infrastructure.Data;
using BoyTechBooking.Infrastructure.Repository;
using BoyTechBooking.Web.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IConcertService, ConcertService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IDbInitial, DbInitial>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
//builder.Services.AddSingleton<PayPalClientFactory>();
builder.Services.AddSingleton<PayPalClientFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new PayPalClientFactory(config);
});
// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Seed the database
DataSeed();

void DataSeed()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbSeedRepo = scope.ServiceProvider.GetRequiredService<IDbInitial>();
        dbSeedRepo.Dataseed();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
