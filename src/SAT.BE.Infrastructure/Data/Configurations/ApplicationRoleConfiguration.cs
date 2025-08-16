using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("Roles");

            // One-to-Many relationship with RolePermissions
            builder.HasMany(r => r.RolePermissions)
                   .WithOne(rp => rp.Role)
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}