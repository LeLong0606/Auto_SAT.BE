using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations
{
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
        public void Configure(EntityTypeBuilder<Shift> builder)
        {
            builder.HasIndex(s => s.ShiftName).IsUnique();

            // One-to-Many relationship with ShiftAssignments
            builder.HasMany(s => s.ShiftAssignments)
                   .WithOne(sa => sa.Shift)
                   .HasForeignKey(sa => sa.ShiftId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}