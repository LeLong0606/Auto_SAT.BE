using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.Scheduling
{
    public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
    {
        public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
        {
            // Primary key
            builder.HasKey(sa => sa.ShiftAssignmentId);

            // Properties
            builder.Property(sa => sa.Date)
                .IsRequired();

            builder.Property(sa => sa.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(ShiftStatus.Scheduled);

            builder.Property(sa => sa.StatusCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("X");

            builder.Property(sa => sa.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(sa => sa.CheckInTime)
                .IsRequired(false);

            builder.Property(sa => sa.CheckOutTime)
                .IsRequired(false);

            builder.Property(sa => sa.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sa => sa.ModifiedDate)
                .IsRequired(false);

            builder.Property(sa => sa.EmployeeId)
                .IsRequired();

            builder.Property(sa => sa.ShiftId)
                .IsRequired();

            // Relationships
            builder.HasOne(sa => sa.Employee)
                .WithMany(e => e.ShiftAssignments)
                .HasForeignKey(sa => sa.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sa => sa.Shift)
                .WithMany(s => s.ShiftAssignments)
                .HasForeignKey(sa => sa.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(sa => new { sa.EmployeeId, sa.Date }).IsUnique();
            builder.HasIndex(sa => sa.Date);
            builder.HasIndex(sa => sa.Status);
            builder.HasIndex(sa => sa.StatusCode);
        }
    }
}
