using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SAT.BE.src.SAT.BE.Domain.Entities.Authentication;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Mappings;
using SAT.BE.src.SAT.BE.Domain.Interfaces;
using SAT.BE.src.SAT.BE.Infrastructure.Repositories;
using SAT.BE.src.SAT.BE.Application.Services.Interfaces;
using SAT.BE.src.SAT.BE.Application.Services.Implementation;
using SAT.BE.src.SAT.BE.Api.Authorization.Handlers;
using SAT.BE.src.SAT.BE.Api.Authorization.Requirements;

namespace SAT.BE.src.SAT.BE.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign in settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // JWT Authentication
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings?.ValidateIssuer ?? true,
                    ValidateAudience = jwtSettings?.ValidateAudience ?? true,
                    ValidateLifetime = jwtSettings?.ValidateLifetime ?? true,
                    ValidateIssuerSigningKey = jwtSettings?.ValidateIssuerSigningKey ?? true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "default-secret-key")),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Authorization with custom policies
            services.AddAuthorization(options =>
            {
                // Permission-based policies
                foreach (var permission in GetAllPermissions())
                {
                    options.AddPolicy($"HasPermission:{permission}", policy =>
                        policy.Requirements.Add(new HasPermissionRequirement(permission)));
                }

                // Role hierarchy-based policies
                foreach (var role in RoleHierarchy.RoleLevels.Keys)
                {
                    options.AddPolicy($"HasMinimumRole:{role}", policy =>
                        policy.Requirements.Add(new HasMinimumRoleRequirement(role)));
                }

                // Team and department access policies
                options.AddPolicy("TeamAccess", policy =>
                    policy.Requirements.Add(new TeamAccessRequirement()));

                options.AddPolicy("DepartmentAccess", policy =>
                    policy.Requirements.Add(new DepartmentAccessRequirement()));

                // Quick access policies for common scenarios
                options.AddPolicy("EmployeeView", policy =>
                    policy.Requirements.Add(new HasPermissionRequirement(PermissionConstants.EMPLOYEE_VIEW)));

                options.AddPolicy("ScheduleCreate", policy =>
                    policy.Requirements.Add(new HasPermissionRequirement(PermissionConstants.SCHEDULE_CREATE)));

                options.AddPolicy("AdminOnly", policy =>
                    policy.Requirements.Add(new HasMinimumRoleRequirement(RoleHierarchy.ADMIN)));

                options.AddPolicy("TeamLeaderOrAbove", policy =>
                    policy.Requirements.Add(new HasMinimumRoleRequirement(RoleHierarchy.TEAM_LEADER)));

                options.AddPolicy("DirectorOrAbove", policy =>
                    policy.Requirements.Add(new HasMinimumRoleRequirement(RoleHierarchy.DIRECTOR)));
            });

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();
            services.AddScoped<IAuthorizationHandler, HasMinimumRoleHandler>();
            services.AddScoped<IAuthorizationHandler, TeamAccessHandler>();
            services.AddScoped<IAuthorizationHandler, DepartmentAccessHandler>();

            // Add HTTP context accessor for authorization handlers
            services.AddHttpContextAccessor();

            // Add AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Add Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Add Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }

        private static IEnumerable<string> GetAllPermissions()
        {
            return new[]
            {
                PermissionConstants.EMPLOYEE_VIEW,
                PermissionConstants.EMPLOYEE_CREATE,
                PermissionConstants.EMPLOYEE_UPDATE,
                PermissionConstants.EMPLOYEE_DELETE,
                PermissionConstants.EMPLOYEE_VIEW_TEAM,
                PermissionConstants.DEPARTMENT_VIEW,
                PermissionConstants.DEPARTMENT_CREATE,
                PermissionConstants.DEPARTMENT_UPDATE,
                PermissionConstants.DEPARTMENT_DELETE,
                PermissionConstants.DEPARTMENT_MANAGE_ALL,
                PermissionConstants.SCHEDULE_VIEW,
                PermissionConstants.SCHEDULE_CREATE,
                PermissionConstants.SCHEDULE_UPDATE,
                PermissionConstants.SCHEDULE_DELETE,
                PermissionConstants.SCHEDULE_CREATE_TEAM,
                PermissionConstants.SCHEDULE_CREATE_ALL,
                PermissionConstants.USER_MANAGEMENT,
                PermissionConstants.ROLE_MANAGEMENT,
                PermissionConstants.SYSTEM_CONFIGURATION,
                PermissionConstants.ATTENDANCE_VIEW,
                PermissionConstants.ATTENDANCE_CREATE,
                PermissionConstants.ATTENDANCE_UPDATE,
                PermissionConstants.ATTENDANCE_VIEW_TEAM,
                PermissionConstants.ATTENDANCE_VIEW_ALL,
                PermissionConstants.REPORT_VIEW,
                PermissionConstants.REPORT_EXPORT,
                PermissionConstants.REPORT_VIEW_TEAM,
                PermissionConstants.REPORT_VIEW_ALL
            };
        }
    }
}