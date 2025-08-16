using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            // Primary key
            builder.HasKey(d => d.DepartmentId);

            // Properties
            builder.Property(d => d.DepartmentCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(d => d.DepartmentName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(d => d.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(d => d.ModifiedDate)
                .IsRequired(false);

            builder.Property(d => d.LeaderId)
                .IsRequired(false);

            // Relationships
            builder.HasOne(d => d.Leader)
                .WithMany(e => e.LeadingDepartments)
                .HasForeignKey(d => d.LeaderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(d => d.DepartmentCode).IsUnique();
            builder.HasIndex(d => d.LeaderId);
            builder.HasIndex(d => d.IsActive);
        }
    }
}
