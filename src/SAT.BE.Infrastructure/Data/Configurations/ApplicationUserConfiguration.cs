using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.EmployeeId).IsUnique().HasFilter("[EmployeeId] IS NOT NULL");

            // One-to-One relationship with Employee
            builder.HasOne(u => u.Employee)
                   .WithOne(e => e.User)
                   .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                   .OnDelete(DeleteBehavior.SetNull);

            // One-to-Many relationship with RefreshTokens
            builder.HasMany(u => u.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}