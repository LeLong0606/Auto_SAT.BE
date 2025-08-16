using SAT.BE.src.SAT.BE.Extensions;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Infrastructure services (DbContext, Identity, JWT, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Swagger / OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Correct versioning for OpenAPI
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SAT.BE API",
        Version = "1.0.0", // <- This is the API version, not the OpenAPI version
        Description = "Staff Attendance Tracking Backend API",
        Contact = new OpenApiContact
        {
            Name = "SAT.BE Development Team",
            Email = "dev@satbe.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Bearer Auth support
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
            Array.Empty<string>()
        }
    });

    // Optional: XML Comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.EnableAnnotations();
});

// Add CORS policies
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

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// DEVELOPMENT ONLY: Swagger, Database Init
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Enable Swagger
    app.UseSwagger(); // <- This automatically generates JSON with "openapi": "3.0.1"
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SAT.BE API v1.0.0");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "SAT.BE API Documentation";
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
        options.EnableDeepLinking();
    });

    // Auto DB Migration & Seeding
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        await DbMigrationHelper.SeedDataAsync(dbContext, logger);
        logger.LogInformation("Database initialized.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed.");
    }
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Global Middleware
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

// Controller routing
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

// Root info endpoint
app.MapGet("/", () => Results.Ok(new
{
    message = "SAT.BE API is running successfully!",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    documentation = "/swagger",
    health = "/health",
    openApiSpec = "/swagger/v1/swagger.json"
}))
.WithName("ApiRoot")
.WithTags("Information");

app.Run();
