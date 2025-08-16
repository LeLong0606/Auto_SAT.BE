using SAT.BE.src.SAT.BE.Extensions;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Infrastructure services (DbContext, Identity, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure API Explorer
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI - Fixed for proper OpenAPI 3.0.1 generation
builder.Services.AddSwaggerGen(options =>
{
    // Primary OpenAPI document configuration with proper versioning
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "SAT.BE API",
        Description = "Staff Attendance Tracking Backend API with Authentication",
        Contact = new OpenApiContact
        {
            Name = "SAT.BE Development Team",
            Email = "dev@satbe.com"
        }
    });

    // JWT Authentication configuration
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Enable annotations for better documentation
    options.EnableAnnotations();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Swagger configuration - Ensure OpenAPI 3.0.1 format
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false; // This ensures OpenAPI 3.0+ format is used
    });
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SAT.BE API v1.0.0");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
        options.EnableDeepLinking();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DefaultModelsExpandDepth(-1);
        options.DocumentTitle = "SAT.BE API Documentation";
    });

    // Database initialization
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Initializing database...");
        await context.Database.MigrateAsync();
        
        // Seed basic data
        await DbMigrationHelper.SeedDataAsync(context, logger);
        
        // Seed roles and admin user
        await DbMigrationHelper.SeedRolesAndAdminAsync(services, logger);
        
        logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security and CORS middleware
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("Health");

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    message = "SAT.BE API with Authentication is running successfully!",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    documentation = "/swagger",
    health = "/health",
    openApiSpec = "/swagger/v1/swagger.json",
    authentication = new
    {
        register = "/api/auth/register",
        login = "/api/auth/login",
        refreshToken = "/api/auth/refresh-token"
    },
    demoAccounts = new
    {
        admin = new { email = "admin@satbe.com", password = "Admin123!" },
        manager = new { email = "manager@satbe.com", password = "Manager123!" },
        hr = new { email = "hr@satbe.com", password = "Hr123!" },
        employee = new { email = "employee@satbe.com", password = "Employee123!" },
        user = new { email = "user@satbe.com", password = "User123!" }
    }
}))
.WithName("ApiRoot")
.WithTags("Information");

app.Run();
