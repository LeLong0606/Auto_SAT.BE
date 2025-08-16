using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using TaskStatusEnum = SAT.BE.src.SAT.BE.Domain.Entities.HR.TaskStatus;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR
{
    public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
    {
        public void Configure(EntityTypeBuilder<TaskAssignment> builder)
        {
            // Primary key
            builder.HasKey(ta => ta.TaskAssignmentId);

            // Properties
            builder.Property(ta => ta.TaskName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ta => ta.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(ta => ta.Priority)
                .IsRequired()
                .HasDefaultValue(TaskPriority.Normal);

            builder.Property(ta => ta.Status)
                .IsRequired()
                .HasDefaultValue(TaskStatusEnum.NotStarted);

            builder.Property(ta => ta.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ta => ta.StartDate)
                .IsRequired(false);

            builder.Property(ta => ta.DueDate)
                .IsRequired(false);

            builder.Property(ta => ta.CompletedDate)
                .IsRequired(false);

            builder.Property(ta => ta.CreatedBy)
                .IsRequired(false);

            // Relationships
            builder.HasMany(ta => ta.EmployeeTaskAssignments)
                .WithOne(eta => eta.TaskAssignment)
                .HasForeignKey(eta => eta.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ta => ta.Status);
            builder.HasIndex(ta => ta.Priority);
            builder.HasIndex(ta => ta.DueDate);
            builder.HasIndex(ta => ta.CreatedBy);
        }
    }
}
