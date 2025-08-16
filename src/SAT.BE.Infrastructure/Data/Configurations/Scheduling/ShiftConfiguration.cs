using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.Scheduling
{
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
        public void Configure(EntityTypeBuilder<Shift> builder)
        {
            // Primary key
            builder.HasKey(s => s.ShiftId);

            // Properties
            builder.Property(s => s.ShiftName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(s => s.StartTime)
                .IsRequired();

            builder.Property(s => s.EndTime)
                .IsRequired();

            builder.Property(s => s.Description)
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Computed column
            builder.Ignore(s => s.IsValidShift);

            // Relationships
            builder.HasMany(s => s.ShiftAssignments)
                .WithOne(sa => sa.Shift)
                .HasForeignKey(sa => sa.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(s => s.Type);
            builder.HasIndex(s => s.StartTime);
            builder.HasIndex(s => s.IsActive);
        }
    }
}
