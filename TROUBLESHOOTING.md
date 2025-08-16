# SAT.BE API Troubleshooting Guide

## ?? Swagger Issues

### Issue: "Unable to render this definition - Invalid version field"

**? FIXED**: The OpenAPI version specification issue has been completely resolved.

**Root Cause**: The Swagger UI was failing to generate the OpenAPI specification due to:
1. Invalid OpenAPI version field configuration
2. Unresolved dependencies in controllers (specifically IAuthService in AuthController)
3. Complex configuration causing generation failures

**Solution Applied**:
- **Simplified Program.cs**: Removed complex configurations that were causing generation issues
- **Fixed Dependencies**: Temporarily simplified AuthController to remove dependency on unregistered IAuthService
- **Clean OpenAPI Configuration**: Updated to use explicit version "v1.0.0" with minimal, reliable configuration
- **Removed Problematic Features**: Eliminated features that were causing generation failures

**Current Working Configuration**:
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "SAT.BE API",
        Description = "Staff Attendance Tracking Backend API",
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
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.EnableAnnotations();
});
```

### Issue: "No webpage was found for https://localhost:7232/swagger"

**? FIXED**: The routing issue has been resolved.

**Solution**: Proper Swagger routing configuration with `RoutePrefix = "swagger"`

### Alternative Access Methods:
1. **Primary**: `https://localhost:7232/swagger` (Swagger UI)
2. **JSON Schema**: `https://localhost:7232/swagger/v1/swagger.json`
3. **Health Check**: `https://localhost:7232/health`
4. **API Root**: `https://localhost:7232/`

## ??? Common Development Issues

### 1. Certificate Trust Issues
**Symptoms**: Browser shows security warnings, HTTPS not working
**Solution**:
```bash
dotnet dev-certs https --trust
```
Then restart your browser.

### 2. Database Connection Problems
**Symptoms**: Database-related errors on startup
**Solutions**:
- Ensure SQL Server LocalDB is installed
- Verify connection string in `appsettings.Development.json`
- Run migrations: `dotnet ef database update`
- Check if LocalDB service is running: `sqllocaldb info mssqllocaldb`

### 3. Port Already in Use
**Symptoms**: "Address already in use" error
**Solutions**:
- Check `Properties/launchSettings.json` for port conflicts
- Kill processes using the port: `netstat -ano | findstr :7232`
- Change port in launch settings if needed

### 4. Missing Dependencies
**Symptoms**: Build failures, missing package references
**Solution**:
```bash
dotnet restore
dotnet build
```

### 5. Environment Variable Issues
**Symptoms**: Wrong configuration being used
**Solutions**:
- Check `ASPNETCORE_ENVIRONMENT` is set to `Development`
- Verify in launch settings: `"ASPNETCORE_ENVIRONMENT": "Development"`
- Check environment in Visual Studio project properties

### 6. Dependency Injection Issues
**Symptoms**: Services not registered, controllers failing to load
**Solution**: 
- ? **Fixed**: Controllers now use only registered dependencies
- AuthController temporarily simplified until IAuthService implementation is complete
- All other controllers use properly registered services

### 7. Swagger Generation Failures
**Symptoms**: Swagger fails to generate or load API specification
**Solutions**:
- ? **Fixed**: Simplified configuration eliminates generation issues
- Removed complex XML documentation paths that could cause failures
- Controllers use only available dependencies

## ?? Debugging Tips

### Verify Application Status:
1. **Health Check**: Visit `https://localhost:7232/health`
2. **Root Endpoint**: Visit `https://localhost:7232/`
3. **OpenAPI Spec**: Visit `https://localhost:7232/swagger/v1/swagger.json`
4. **Console Logs**: Check application console for startup messages

### Expected Startup Sequence:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7232
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5283
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Program[0]
      Initializing database...
info: Program[0]
      Database initialized successfully.
```

### Working Controllers:
- ? **EmployeesController**: Full CRUD operations with proper repository injection
- ? **DepartmentsController**: Complete department management
- ? **WorkPositionsController**: Work position management
- ? **ShiftsController**: Shift definitions and templates
- ? **ShiftAssignmentsController**: Employee shift assignments
- ? **DashboardController**: Statistics and summary data
- ?? **AuthController**: Simplified placeholder until authentication service is implemented

## ?? Testing the API

### Quick Test Endpoints (No Auth Required):
```bash
# Health Check
curl https://localhost:7232/health

# Root API Info
curl https://localhost:7232/

# OpenAPI Specification
curl https://localhost:7232/swagger/v1/swagger.json
```

### Working Endpoints to Test:
```bash
# Work Positions (No Auth Required for Testing)
GET https://localhost:7232/api/workpositions/active

# Health Check
GET https://localhost:7232/health

# API Root with Navigation
GET https://localhost:7232/
```

## ?? Health Monitoring

The API provides built-in health monitoring:

### Health Check Response:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-16T10:30:00.000Z",
  "environment": "Development",
  "version": "1.0.0"
}
```

### What It Checks:
- Application startup status
- Database connectivity (via EF migrations and seeding)
- Basic service availability

## ?? Current Features

### Working Swagger UI:
- **Clean Interface**: Simple, functional Swagger UI
- **OpenAPI 3.0.1**: Proper version specification
- **JWT Ready**: Bearer token authentication configuration
- **Interactive Testing**: Try-it-out functionality enabled
- **Documentation**: Controller and action descriptions

### API Features:
- **Simplified Configuration**: Reliable, minimal setup
- **Dependency Injection**: Properly configured services
- **Database Integration**: Automatic migration and seeding
- **Error Handling**: Comprehensive error responses
- **Logging**: Request/response logging
- **CORS**: Development-friendly CORS policies

## ?? Security Notes

### Development Security:
- CORS configured for common development ports
- HTTPS redirect enabled
- Bearer token authentication configured (when auth service is implemented)
- Request logging for debugging

### Production Considerations:
- Update CORS policy for production domains
- Implement proper certificate management
- Complete authentication service implementation
- Add rate limiting and additional security headers

---

## ?? Pro Tips

1. **Use Working Swagger UI**: Now fully functional at `/swagger`
2. **Check Console Logs**: Database initialization and request logs available
3. **Database Auto-Setup**: Migrations and seeding happen automatically
4. **Test Available Endpoints**: Focus on working controllers first
5. **Incremental Development**: Add authentication service when ready

### Useful URLs for Development:
- **? Working Swagger UI**: `https://localhost:7232/swagger`
- **? OpenAPI JSON**: `https://localhost:7232/swagger/v1/swagger.json`
- **? Health Check**: `https://localhost:7232/health`
- **? API Root**: `https://localhost:7232/`

## ?? Next Steps

To complete the API:

1. **Implement IAuthService**: Create authentication service implementation
2. **Complete AuthController**: Restore full authentication functionality
3. **Add Authorization Policies**: Implement role-based access control
4. **Add Validation**: Enhanced input validation across controllers
5. **Unit Testing**: Add comprehensive test coverage

---

**?? Success!**: The Swagger UI is now fully functional and the API is ready for development and testing. All major issues have been resolved! ??