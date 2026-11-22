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

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(booking => booking.Price).HasPrecision(10, 2);

            entity.HasIndex(booking => new { booking.ClientId, booking.Status });

            entity.HasIndex(booking => booking.AvailabilitySlotId)
                .IsUnique()
                .HasFilter("[Status] <> 'Cancelled'");

            entity.ToTable(table => table.HasTrigger("trg_Bookings_StatusHistory"));

            entity.HasOne(booking => booking.Client)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(booking => booking.AvailabilitySlot)
                .WithMany(slot => slot.Bookings)
                .HasForeignKey(booking => booking.AvailabilitySlotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(booking => booking.EquipmentCategory)
                .WithMany(category => category.Bookings)
                .HasForeignKey(booking => booking.EquipmentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConsultationReview>(entity =>
        {
            entity.HasOne(review => review.Booking)
                .WithOne(booking => booking.Review)
                .HasForeignKey<ConsultationReview>(review => review.BookingId);

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_ConsultationReviews_Rating",
                "[Rating] BETWEEN 1 AND 5"));
        });

        modelBuilder.Entity<BookingStatusHistory>(entity =>
        {
            entity.Property(history => history.OldStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(history => history.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(20);
        });
    }
}