using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Microsoft.AspNetCore.Identity;
using Soundcheck.Web.Models;
using Soundcheck.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddUserManager<ApplicationUserManager>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<IConsultantRatingService, ConsultantRatingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseStatusCodePagesWithReExecute("/Home/StatusPage", "?code={0}");

app.Run();