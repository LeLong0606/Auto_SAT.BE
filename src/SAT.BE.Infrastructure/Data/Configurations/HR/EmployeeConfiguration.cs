using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            // Primary key
            builder.HasKey(e => e.EmployeeId);

            // Properties
            builder.Property(e => e.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.DateOfBirth)
                .IsRequired();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ModifiedDate)
                .IsRequired(false);

            builder.Property(e => e.DepartmentId)
                .IsRequired();

            builder.Property(e => e.WorkPositionId)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.WorkPosition)
                .WithMany(wp => wp.Employees)
                .HasForeignKey(e => e.WorkPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-One relationship with ApplicationUser (ApplicationUser has the foreign key)
            builder.HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(e => e.ShiftAssignments)
                .WithOne(sa => sa.Employee)
                .HasForeignKey(sa => sa.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.EmployeeTaskAssignments)
                .WithOne(eta => eta.Employee)
                .HasForeignKey(eta => eta.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.LeadingDepartments)
                .WithOne(d => d.Leader)
                .HasForeignKey(d => d.LeaderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(e => e.EmployeeCode).IsUnique();
            builder.HasIndex(e => e.Email).IsUnique();
            builder.HasIndex(e => e.DepartmentId);
            builder.HasIndex(e => e.WorkPositionId);
            builder.HasIndex(e => e.IsActive);
        }
    }
}
