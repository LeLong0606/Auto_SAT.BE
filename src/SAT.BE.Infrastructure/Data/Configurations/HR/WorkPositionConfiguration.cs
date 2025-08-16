using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR
{
    public class WorkPositionConfiguration : IEntityTypeConfiguration<WorkPosition>
    {
        public void Configure(EntityTypeBuilder<WorkPosition> builder)
        {
            // Primary key
            builder.HasKey(wp => wp.WorkPositionId);

            // Properties
            builder.Property(wp => wp.PositionCode)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(wp => wp.PositionName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(wp => wp.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(wp => wp.Level)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(wp => wp.BaseSalary)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(wp => wp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(wp => wp.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasMany(wp => wp.Employees)
                .WithOne(e => e.WorkPosition)
                .HasForeignKey(e => e.WorkPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(wp => wp.PositionCode).IsUnique();
            builder.HasIndex(wp => wp.Level);
            builder.HasIndex(wp => wp.IsActive);
        }
    }
}
