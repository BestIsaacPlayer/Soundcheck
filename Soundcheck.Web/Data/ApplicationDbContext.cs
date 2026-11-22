using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ConsultantProfile> ConsultantProfiles => Set<ConsultantProfile>();

    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();

    public DbSet<ConsultantCategory> ConsultantCategories => Set<ConsultantCategory>();

    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<ConsultationReview> ConsultationReviews => Set<ConsultationReview>();

    public DbSet<BookingStatusHistory> BookingStatusHistory => Set<BookingStatusHistory>();
}