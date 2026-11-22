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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConsultantProfile>(entity =>
        {
            entity.Property(profile => profile.HourlyRate).HasPrecision(10, 2);

            entity.HasOne(profile => profile.User)
                .WithOne(user => user.ConsultantProfile)
                .HasForeignKey<ConsultantProfile>(profile => profile.UserId);
        });

        modelBuilder.Entity<EquipmentCategory>(entity => { entity.HasIndex(category => category.Name).IsUnique(); });

        modelBuilder.Entity<ConsultantCategory>(entity =>
        {
            entity.HasKey(consultantCategory => new
            {
                consultantCategory.ConsultantProfileId,
                consultantCategory.EquipmentCategoryId
            });
        });

        modelBuilder.Entity<AvailabilitySlot>(entity =>
        {
            entity.HasIndex(slot => new { slot.ConsultantProfileId, slot.StartTime });

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_AvailabilitySlots_EndAfterStart",
                "[EndTime] > [StartTime]"));
        });
    }
}