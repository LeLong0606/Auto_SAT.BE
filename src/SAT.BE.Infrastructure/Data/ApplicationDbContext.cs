using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;
using SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.HR;
using SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.Identity;
using SAT.BE.src.SAT.BE.Infrastructure.Data.Configurations.Scheduling;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        #region Identity DbSets
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        #endregion

        #region HR DbSets
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<WorkPosition> WorkPositions { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<EmployeeTaskAssignment> EmployeeTaskAssignments { get; set; }
        #endregion

        #region Scheduling DbSets
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names for Identity tables
            ConfigureIdentityTableNames(modelBuilder);

            // Apply all configurations
            ApplyConfigurations(modelBuilder);
        }

        private void ConfigureIdentityTableNames(ModelBuilder modelBuilder)
        {
            // Customize Identity table names
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        }

        private void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            // Identity Configurations
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

            // HR Configurations
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
            modelBuilder.ApplyConfiguration(new WorkPositionConfiguration());
            modelBuilder.ApplyConfiguration(new TaskAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeTaskAssignmentConfiguration());

            // Scheduling Configurations
            modelBuilder.ApplyConfiguration(new ShiftConfiguration());
            modelBuilder.ApplyConfiguration(new ShiftAssignmentConfiguration());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedDate").CurrentValue == null)
                    {
                        entry.Property("CreatedDate").CurrentValue = DateTime.UtcNow;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Property("ModifiedDate") != null)
                    {
                        entry.Property("ModifiedDate").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
