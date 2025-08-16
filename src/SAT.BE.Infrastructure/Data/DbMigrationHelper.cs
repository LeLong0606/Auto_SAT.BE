using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Infrastructure.Data
{
    public static class DbMigrationHelper
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting database seeding...");

                // Seed Roles
                await SeedRolesAsync(context, logger);

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

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                logger.LogInformation("Starting roles and admin user seeding...");

                // Create roles with hierarchy
                var roles = new[] { "SuperAdmin", "Admin", "Director", "Manager", "TeamLeader", "HR", "Employee", "User" };

                foreach (var roleName in roles)
                {
                    var roleExists = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        var role = new ApplicationRole
                        {
                            Name = roleName,
                            Description = GetRoleDescription(roleName),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };

                        var result = await roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            logger.LogInformation("Role {RoleName} created successfully", roleName);
                        }
                        else
                        {
                            logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // Create default admin user
                var adminEmail = "admin@satbe.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Administrator",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Create demo users with proper roles
                await CreateDemoUsersAsync(userManager, context, logger);

                // Seed demo employees first so they can be linked to users
                await SeedDemoEmployees(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding roles and admin user.");
            }
        }

        private static string GetRoleDescription(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => "Super Administrator with full system access",
                "Admin" => "Administrator with system management access",
                "Director" => "Director with full departmental access and can create shifts for all employees",
                "Manager" => "Manager with departmental access and team management",
                "TeamLeader" => "Team Leader who can view team members and assign shifts to team members only",
                "HR" => "Human Resources with employee management access",
                "Employee" => "Employee with limited access to personal data and schedules",
                "User" => "Basic user with read-only access",
                _ => $"{roleName} role with appropriate permissions"
            };
        }

        private static async Task CreateDemoUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Demo users with proper role hierarchy
                var demoUsers = new[]
                {
                    new { Email = "director@satbe.com", FullName = "Robert Director", Role = "Director", Password = "Director123!" },
                    new { Email = "manager@satbe.com", FullName = "John Manager", Role = "Manager", Password = "Manager123!" },
                    new { Email = "teamleader@satbe.com", FullName = "Sarah TeamLeader", Role = "TeamLeader", Password = "TeamLeader123!" },
                    new { Email = "hr@satbe.com", FullName = "Jane HR", Role = "HR", Password = "Hr123!" },
                    new { Email = "employee@satbe.com", FullName = "Bob Employee", Role = "Employee", Password = "Employee123!" },
                    new { Email = "user@satbe.com", FullName = "Alice User", Role = "User", Password = "User123!" }
                };

                foreach (var userData in demoUsers)
                {
                    var existingUser = await userManager.FindByEmailAsync(userData.Email);
                    if (existingUser == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userData.Email,
                            Email = userData.Email,
                            FullName = userData.FullName,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(user, userData.Password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, userData.Role);
                            await userManager.AddToRoleAsync(user, "User"); // All users get basic User role
                            logger.LogInformation("Demo user created: {Email} with role {Role}", userData.Email, userData.Role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating demo users");
            }
        }

        private static async Task SeedDemoEmployees(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                if (!await context.Employees.AnyAsync())
                {
                    logger.LogInformation("Seeding demo employees...");

                    // Ensure we have departments and work positions
                    var itDepartment = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "IT");
                    var hrDepartment = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "HR");
                    var finDepartment = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "FIN");

                    var devPosition = await context.WorkPositions.FirstOrDefaultAsync(w => w.PositionCode == "DEV");
                    var pmPosition = await context.WorkPositions.FirstOrDefaultAsync(w => w.PositionCode == "PM");
                    var tlPosition = await context.WorkPositions.FirstOrDefaultAsync(w => w.PositionCode == "TL");

                    if (itDepartment != null && devPosition != null && pmPosition != null && tlPosition != null)
                    {
                        var employees = new[]
                        {
                            new Domain.Entities.HR.Employee
                            {
                                EmployeeCode = "EMP001",
                                FullName = "John Developer",
                                DateOfBirth = new DateTime(1990, 1, 15),
                                Email = "john.dev@satbe.com",
                                PhoneNumber = "+1234567890",
                                DepartmentId = itDepartment.DepartmentId,
                                WorkPositionId = devPosition.WorkPositionId,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow
                            },
                            new Domain.Entities.HR.Employee
                            {
                                EmployeeCode = "EMP002",
                                FullName = "Sarah TeamLeader",
                                DateOfBirth = new DateTime(1988, 3, 20),
                                Email = "sarah.tl@satbe.com",
                                PhoneNumber = "+1234567891",
                                DepartmentId = itDepartment.DepartmentId,
                                WorkPositionId = tlPosition.WorkPositionId,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow
                            },
                            new Domain.Entities.HR.Employee
                            {
                                EmployeeCode = "EMP003",
                                FullName = "Robert Director",
                                DateOfBirth = new DateTime(1985, 7, 10),
                                Email = "robert.dir@satbe.com",
                                PhoneNumber = "+1234567892",
                                DepartmentId = itDepartment.DepartmentId,
                                WorkPositionId = pmPosition.WorkPositionId,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow
                            }
                        };

                        await context.Employees.AddRangeAsync(employees);
                        await context.SaveChangesAsync();

                        // Make Sarah TeamLeader the leader of IT department
                        var sarahEmployee = employees.First(e => e.EmployeeCode == "EMP002");
                        itDepartment.LeaderId = sarahEmployee.EmployeeId;
                        await context.SaveChangesAsync();

                        logger.LogInformation("Demo employees seeded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding demo employees");
            }
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!await context.Roles.AnyAsync())
            {
                logger.LogInformation("Seeding basic roles...");

                var roles = new[]
                {
                    new ApplicationRole
                    {
                        Name = "SuperAdmin",
                        Description = "Super Administrator with full system access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new ApplicationRole
                    {
                        Name = "Admin",
                        Description = "Administrator with system management access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new ApplicationRole
                    {
                        Name = "Manager",
                        Description = "Manager with team and department access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new ApplicationRole
                    {
                        Name = "HR",
                        Description = "Human Resources with employee management access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new ApplicationRole
                    {
                        Name = "Employee",
                        Description = "Employee with limited access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new ApplicationRole
                    {
                        Name = "User",
                        Description = "Basic user with read-only access",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                logger.LogInformation("Basic roles seeded successfully.");
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