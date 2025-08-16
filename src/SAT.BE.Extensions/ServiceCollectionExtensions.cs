using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            // Add AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Add Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Add Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }
}