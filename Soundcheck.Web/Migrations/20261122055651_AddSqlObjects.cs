using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soundcheck.Web.Migrations
{
    public partial class AddSqlObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateObject(migrationBuilder, """
                CREATE FUNCTION dbo.fn_ConsultantAverageRating (@ConsultantProfileId INT)
                RETURNS DECIMAL(3, 2)
                AS
                BEGIN
                    DECLARE @AverageRating DECIMAL(3, 2);

                    SELECT @AverageRating = CAST(AVG(CAST(review.Rating AS DECIMAL(5, 2))) AS DECIMAL(3, 2))
                    FROM dbo.ConsultationReviews AS review
                    INNER JOIN dbo.Bookings AS booking ON booking.Id = review.BookingId
                    INNER JOIN dbo.AvailabilitySlots AS slot ON slot.Id = booking.AvailabilitySlotId
                    WHERE slot.ConsultantProfileId = @ConsultantProfileId;

                    RETURN @AverageRating;
                END
                """);

            CreateObject(migrationBuilder, """
                CREATE PROCEDURE dbo.sp_BookConsultation
                    @ClientId NVARCHAR(450),
                    @AvailabilitySlotId INT,
                    @EquipmentCategoryId INT,
                    @Topic NVARCHAR(200),
                    @BookingId INT OUTPUT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        DECLARE @ConsultantProfileId INT;
                        DECLARE @StartTime DATETIME2;
                        DECLARE @EndTime DATETIME2;
                        DECLARE @HourlyRate DECIMAL(10, 2);
                        DECLARE @Price DECIMAL(10, 2);

                        SELECT
                            @ConsultantProfileId = slot.ConsultantProfileId,
                            @StartTime = slot.StartTime,
                            @EndTime = slot.EndTime
                        FROM dbo.AvailabilitySlots AS slot WITH (UPDLOCK, HOLDLOCK)
                        WHERE slot.Id = @AvailabilitySlotId AND slot.IsBooked = 0;

                        IF @ConsultantProfileId IS NULL
                            THROW 50001, 'The selected slot does not exist or is already booked.', 1;

                        IF @StartTime <= SYSDATETIME()
                            THROW 50002, 'The selected slot is in the past.', 1;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM dbo.ConsultantCategories AS consultantCategory
                            WHERE consultantCategory.ConsultantProfileId = @ConsultantProfileId
                                AND consultantCategory.EquipmentCategoryId = @EquipmentCategoryId)
                            THROW 50003, 'The consultant does not offer the selected equipment category.', 1;

                        SELECT @HourlyRate = consultantProfile.HourlyRate
                        FROM dbo.ConsultantProfiles AS consultantProfile
                        WHERE consultantProfile.Id = @ConsultantProfileId;

                        SET @Price = CAST(ROUND(@HourlyRate * DATEDIFF(MINUTE, @StartTime, @EndTime) / 60.0, 2) AS DECIMAL(10, 2));

                        INSERT INTO dbo.Bookings (ClientId, AvailabilitySlotId, EquipmentCategoryId, Topic, Status, Price, CreatedAt)
                        VALUES (@ClientId, @AvailabilitySlotId, @EquipmentCategoryId, @Topic, N'Pending', @Price, SYSUTCDATETIME());

                        SET @BookingId = SCOPE_IDENTITY();

                        UPDATE dbo.AvailabilitySlots
                        SET IsBooked = 1
                        WHERE Id = @AvailabilitySlotId;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;

                        THROW;
                    END CATCH
                END
                """);

            CreateObject(migrationBuilder, """
                CREATE PROCEDURE dbo.sp_CancelBooking
                    @BookingId INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        DECLARE @CurrentStatus NVARCHAR(20);
                        DECLARE @AvailabilitySlotId INT;

                        SELECT
                            @CurrentStatus = booking.Status,
                            @AvailabilitySlotId = booking.AvailabilitySlotId
                        FROM dbo.Bookings AS booking WITH (UPDLOCK)
                        WHERE booking.Id = @BookingId;

                        IF @CurrentStatus IS NULL
                            THROW 50010, 'The booking does not exist.', 1;

                        IF @CurrentStatus NOT IN (N'Pending', N'Confirmed')
                            THROW 50011, 'Only pending or confirmed bookings can be cancelled.', 1;

                        UPDATE dbo.Bookings
                        SET Status = N'Cancelled'
                        WHERE Id = @BookingId;

                        UPDATE dbo.AvailabilitySlots
                        SET IsBooked = 0
                        WHERE Id = @AvailabilitySlotId;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;

                        THROW;
                    END CATCH
                END
                """);

            CreateObject(migrationBuilder, """
                CREATE VIEW dbo.vw_ConsultantRevenueReport
                AS
                SELECT
                    consultantProfile.Id AS ConsultantProfileId,
                    consultantUser.FullName AS ConsultantName,
                    equipmentCategory.Id AS EquipmentCategoryId,
                    equipmentCategory.Name AS CategoryName,
                    COUNT(booking.Id) AS CompletedConsultations,
                    SUM(booking.Price) AS TotalRevenue
                FROM dbo.Bookings AS booking
                INNER JOIN dbo.AvailabilitySlots AS slot ON slot.Id = booking.AvailabilitySlotId
                INNER JOIN dbo.ConsultantProfiles AS consultantProfile ON consultantProfile.Id = slot.ConsultantProfileId
                INNER JOIN dbo.AspNetUsers AS consultantUser ON consultantUser.Id = consultantProfile.UserId
                INNER JOIN dbo.EquipmentCategories AS equipmentCategory ON equipmentCategory.Id = booking.EquipmentCategoryId
                WHERE booking.Status = N'Completed'
                GROUP BY
                    consultantProfile.Id,
                    consultantUser.FullName,
                    equipmentCategory.Id,
                    equipmentCategory.Name
                """);

            CreateObject(migrationBuilder, """
                CREATE TRIGGER dbo.trg_Bookings_StatusHistory
                ON dbo.Bookings
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO dbo.BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedAt)
                    SELECT inserted.Id, deleted.Status, inserted.Status, SYSUTCDATETIME()
                    FROM inserted
                    INNER JOIN deleted ON deleted.Id = inserted.Id
                    WHERE inserted.Status <> deleted.Status;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_Bookings_StatusHistory;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_ConsultantRevenueReport;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_CancelBooking;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_BookConsultation;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_ConsultantAverageRating;");
        }

        private static void CreateObject(MigrationBuilder migrationBuilder, string createStatement)
        {
            var escapedStatement = createStatement.Replace("'", "''");
            migrationBuilder.Sql($"EXEC(N'{escapedStatement}')");
        }
    }
}
