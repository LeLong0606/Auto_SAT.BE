using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;
using Microsoft.AspNetCore.Identity;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data
{
    public static class DbMigrationHelper
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting database seeding...");

                // Seed Departments
                if (!await context.Departments.AnyAsync())
                {
                    logger.LogInformation("Seeding departments...");
                    await SeedDepartmentsAsync(context);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Departments seeded successfully.");
                }

                // Seed Work Positions
                if (!await context.WorkPositions.AnyAsync())
                {
                    logger.LogInformation("Seeding work positions...");
                    await SeedWorkPositionsAsync(context);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Work positions seeded successfully.");
                }

                // Seed Permissions
                if (!await context.Permissions.AnyAsync())
                {
                    logger.LogInformation("Seeding permissions...");
                    await SeedPermissionsAsync(context);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Permissions seeded successfully.");
                }

                // Seed Shifts
                if (!await context.Shifts.AnyAsync())
                {
                    logger.LogInformation("Seeding shifts...");
                    await SeedShiftsAsync(context);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Shifts seeded successfully.");
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
        {
            var departments = new[]
            {
                new Department
                {
                    DepartmentCode = "IT",
                    DepartmentName = "Information Technology",
                    Description = "IT Department",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    DepartmentCode = "HR",
                    DepartmentName = "Human Resources",
                    Description = "HR Department",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    DepartmentCode = "FIN",
                    DepartmentName = "Finance",
                    Description = "Finance Department",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    DepartmentCode = "MKT",
                    DepartmentName = "Marketing",
                    Description = "Marketing Department",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.Departments.AddRangeAsync(departments);
        }

        private static async Task SeedWorkPositionsAsync(ApplicationDbContext context)
        {
            var positions = new[]
            {
                new WorkPosition
                {
                    PositionCode = "DEV",
                    PositionName = "Developer",
                    Description = "Software Developer",
                    Level = 2,
                    BaseSalary = 50000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new WorkPosition
                {
                    PositionCode = "PM",
                    PositionName = "Project Manager",
                    Description = "Project Manager",
                    Level = 4,
                    BaseSalary = 75000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new WorkPosition
                {
                    PositionCode = "TL",
                    PositionName = "Team Leader",
                    Description = "Team Leader",
                    Level = 3,
                    BaseSalary = 60000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new WorkPosition
                {
                    PositionCode = "INT",
                    PositionName = "Intern",
                    Description = "Intern",
                    Level = 1,
                    BaseSalary = 20000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.WorkPositions.AddRangeAsync(positions);
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            var permissions = new[]
            {
                // HR Permissions
                new Permission
                {
                    PermissionName = "View Employees",
                    PermissionCode = PermissionConstants.EMPLOYEE_VIEW,
                    Description = "Can view employee information",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Create Employees",
                    PermissionCode = PermissionConstants.EMPLOYEE_CREATE,
                    Description = "Can create new employees",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Update Employees",
                    PermissionCode = PermissionConstants.EMPLOYEE_UPDATE,
                    Description = "Can update employee information",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Delete Employees",
                    PermissionCode = PermissionConstants.EMPLOYEE_DELETE,
                    Description = "Can delete employees",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                
                // Department Permissions
                new Permission
                {
                    PermissionName = "View Departments",
                    PermissionCode = PermissionConstants.DEPARTMENT_VIEW,
                    Description = "Can view department information",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Manage Departments",
                    PermissionCode = PermissionConstants.DEPARTMENT_MANAGE_ALL,
                    Description = "Can manage all department operations",
                    Category = PermissionCategories.HR,
                    IsActive = true
                },
                
                // Scheduling Permissions
                new Permission
                {
                    PermissionName = "View Schedule",
                    PermissionCode = PermissionConstants.SCHEDULE_VIEW,
                    Description = "Can view schedules",
                    Category = PermissionCategories.SCHEDULING,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Create Schedule",
                    PermissionCode = PermissionConstants.SCHEDULE_CREATE,
                    Description = "Can create schedules",
                    Category = PermissionCategories.SCHEDULING,
                    IsActive = true
                },
                
                // System Permissions
                new Permission
                {
                    PermissionName = "User Management",
                    PermissionCode = PermissionConstants.USER_MANAGEMENT,
                    Description = "Can manage user accounts",
                    Category = PermissionCategories.SYSTEM,
                    IsActive = true
                },
                new Permission
                {
                    PermissionName = "Role Management",
                    PermissionCode = PermissionConstants.ROLE_MANAGEMENT,
                    Description = "Can manage roles",
                    Category = PermissionCategories.SYSTEM,
                    IsActive = true
                }
            };

            await context.Permissions.AddRangeAsync(permissions);
        }

        private static async Task SeedShiftsAsync(ApplicationDbContext context)
        {
            var shifts = new[]
            {
                new Shift
                {
                    ShiftName = "Morning Shift",
                    Type = ShiftType.Morning,
                    StartTime = new TimeSpan(8, 0, 0),   // 08:00
                    EndTime = new TimeSpan(16, 0, 0),    // 16:00
                    Description = "Standard morning shift",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Shift
                {
                    ShiftName = "Afternoon Shift",
                    Type = ShiftType.Afternoon,
                    StartTime = new TimeSpan(14, 0, 0),  // 14:00
                    EndTime = new TimeSpan(22, 0, 0),    // 22:00
                    Description = "Standard afternoon shift",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Shift
                {
                    ShiftName = "Night Shift",
                    Type = ShiftType.Night,
                    StartTime = new TimeSpan(22, 0, 0),  // 22:00
                    EndTime = new TimeSpan(6, 0, 0),     // 06:00
                    Description = "Standard night shift",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.Shifts.AddRangeAsync(shifts);
        }
    }
}