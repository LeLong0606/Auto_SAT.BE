using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR
{
    public class EmployeeTaskAssignmentConfiguration : IEntityTypeConfiguration<EmployeeTaskAssignment>
    {
        public void Configure(EntityTypeBuilder<EmployeeTaskAssignment> builder)
        {
            // Primary key
            builder.HasKey(eta => eta.EmployeeTaskAssignmentId);

            // Properties
            builder.Property(eta => eta.EmployeeId)
                .IsRequired();

            builder.Property(eta => eta.TaskAssignmentId)
                .IsRequired();

            builder.Property(eta => eta.AssignedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(eta => eta.StartedDate)
                .IsRequired(false);

            builder.Property(eta => eta.CompletedDate)
                .IsRequired(false);

            builder.Property(eta => eta.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(eta => eta.CompletionNotes)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(eta => eta.ProgressPercentage)
                .IsRequired()
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(eta => eta.Employee)
                .WithMany(e => e.EmployeeTaskAssignments)
                .HasForeignKey(eta => eta.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(eta => eta.TaskAssignment)
                .WithMany(ta => ta.EmployeeTaskAssignments)
                .HasForeignKey(eta => eta.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(eta => new { eta.EmployeeId, eta.TaskAssignmentId }).IsUnique();
            builder.HasIndex(eta => eta.AssignedDate);
            builder.HasIndex(eta => eta.ProgressPercentage);
        }
    }
}
