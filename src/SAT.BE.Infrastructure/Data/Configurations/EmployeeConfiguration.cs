using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasIndex(e => e.EmployeeCode).IsUnique();
            builder.HasIndex(e => e.Email).IsUnique();

            // Many-to-One relationship with Department
            builder.HasOne(e => e.Department)
                   .WithMany(d => d.Employees)
                   .HasForeignKey(e => e.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Many-to-One relationship with WorkPosition
            builder.HasOne(e => e.WorkPosition)
                   .WithMany(wp => wp.Employees)
                   .HasForeignKey(e => e.WorkPositionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many relationship with ShiftAssignments
            builder.HasMany(e => e.ShiftAssignments)
                   .WithOne(sa => sa.Employee)
                   .HasForeignKey(sa => sa.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many relationship with EmployeeTaskAssignments
            builder.HasMany(e => e.EmployeeTaskAssignments)
                   .WithOne(eta => eta.Employee)
                   .HasForeignKey(eta => eta.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many relationship with LeadingDepartments (self-referencing)
            builder.HasMany(e => e.LeadingDepartments)
                   .WithOne(d => d.Leader)
                   .HasForeignKey(d => d.LeaderId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}