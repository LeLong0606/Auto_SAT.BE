# SAT.BE API Controllers

## Overview
This document describes the API controllers implemented in the Staff Attendance Tracking (SAT.BE) backend application.

## Quick Start Guide

### ?? Running the Application
1. **Start the application**: Run `dotnet run` or press F5 in Visual Studio
2. **Access Enhanced Swagger UI**: Navigate to `https://localhost:7232/swagger` 
3. **API Root**: Visit `https://localhost:7232/` for comprehensive API information
4. **Health Check**: Check `https://localhost:7232/health` for detailed application status
5. **OpenAPI Spec**: Download from `https://localhost:7232/swagger/v1/swagger.json`

### ?? Key URLs
- **?? Enhanced Swagger UI**: `https://localhost:7232/swagger`
- **?? OpenAPI Specification**: `https://localhost:7232/swagger/v1/swagger.json`
- **?? API Root**: `https://localhost:7232/`
- **?? Health Check**: `https://localhost:7232/health`
- **?? Base API Path**: `https://localhost:7232/api/`

### ? New Features
- **OpenAPI 3.0.1 Compliance**: Latest specification support
- **Enhanced UI**: Professional styling with custom CSS
- **Interactive Testing**: Try-it-out enabled by default
- **JWT Authentication**: Built-in Bearer token authentication
- **Auto-generated Documentation**: XML comments integration
- **Dark Mode Support**: Automatic theme detection
- **Deep Linking**: Shareable endpoint URLs
- **Request Duration**: Performance monitoring

## Controllers Created

### 1. AuthController (`/api/auth`) ??
Handles user authentication and authorization operations.

**Endpoints:**
- `POST /api/auth/login` - User login with JWT token generation
- `POST /api/auth/logout` - User logout and token invalidation
- `POST /api/auth/refresh-token` - Refresh JWT token
- `POST /api/auth/change-password` - Change user password
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `GET /api/auth/me` - Get current user information

### 2. EmployeesController (`/api/employees`) ??
Manages employee CRUD operations and related functionality.

**Endpoints:**
- `GET /api/employees` - Get employees with pagination
- `GET /api/employees/{id}` - Get employee by ID
- `GET /api/employees/by-code/{code}` - Get employee by employee code
- `GET /api/employees/department/{departmentId}` - Get employees by department
- `POST /api/employees` - Create new employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee
- `GET /api/employees/check-code/{code}` - Check if employee code exists

### 3. DepartmentsController (`/api/departments`) ??
Manages department operations and organizational structure.

**Endpoints:**
- `GET /api/departments` - Get departments with pagination
- `GET /api/departments/active` - Get active departments (for dropdowns)
- `GET /api/departments/{id}` - Get department by ID
- `GET /api/departments/by-code/{code}` - Get department by code
- `POST /api/departments` - Create new department
- `PUT /api/departments/{id}` - Update department
- `DELETE /api/departments/{id}` - Delete department
- `GET /api/departments/check-code/{code}` - Check if department code exists

### 4. WorkPositionsController (`/api/workpositions`) ??
Manages work position/job title operations.

**Endpoints:**
- `GET /api/workpositions` - Get work positions with pagination
- `GET /api/workpositions/active` - Get active work positions
- `GET /api/workpositions/{id}` - Get work position by ID
- `GET /api/workpositions/by-code/{code}` - Get work position by code
- `GET /api/workpositions/by-level/{level}` - Get work positions by level

### 5. ShiftsController (`/api/shifts`) ??
Manages shift definitions and scheduling templates.

**Endpoints:**
- `GET /api/shifts` - Get all shifts
- `GET /api/shifts/{id}` - Get shift by ID
- `GET /api/shifts/by-type/{type}` - Get shifts by type (Morning=0, Afternoon=1, Night=2)
- `GET /api/shifts/active` - Get active shifts

### 6. ShiftAssignmentsController (`/api/shiftassignments`) ??
Manages employee shift assignments and attendance tracking.

**Endpoints:**
- `GET /api/shiftassignments` - Get shift assignments with filtering and pagination
- `GET /api/shiftassignments/employee/{employeeId}` - Get assignments for specific employee
- `GET /api/shiftassignments/{id}` - Get shift assignment by ID
- `GET /api/shiftassignments/today` - Get today's shift assignments

### 7. DashboardController (`/api/dashboard`) ??
Provides statistics and summary data for dashboard views.

**Endpoints:**
- `GET /api/dashboard/statistics` - Get general dashboard statistics
- `GET /api/dashboard/employee-by-department` - Get employee count by department
- `GET /api/dashboard/employee-by-position` - Get employee count by work position
- `GET /api/dashboard/attendance-today` - Get today's attendance summary

## Features Implemented

### ? Authentication & Authorization
- JWT-based authentication with Bearer tokens
- Role-based authorization with `[Authorize]` attributes
- Comprehensive password management (change, reset, forgot password)
- Token refresh mechanism with security validation
- Built-in Swagger UI authentication integration

### ? Comprehensive CRUD Operations
- Full Create, Read, Update, Delete operations for core entities
- Advanced pagination support with metadata
- Multi-criteria filtering and search capabilities
- Data validation using Data Annotations
- Optimistic concurrency control

### ? Error Handling & Logging
- Structured error responses using `ServiceResult<T>` pattern
- Comprehensive logging for all operations with structured data
- Proper HTTP status codes with meaningful error messages
- Exception handling with user-friendly error responses
- Request/response logging middleware

### ? Data Transfer Objects (DTOs)
- Clean Request/Response DTOs for API contracts
- AutoMapper integration for seamless entity-DTO mapping
- Input validation on all request DTOs
- Consistent response formats across all endpoints
- Comprehensive validation error handling

### ? Repository Pattern Implementation
- Clean separation of concerns with repository abstraction
- Generic repository interfaces and implementations
- Dependency injection for improved testability
- Entity Framework Core integration with optimizations
- Unit of Work pattern support

### ? Enhanced API Documentation
- **OpenAPI 3.0.1 Compliance**: Latest specification support
- **Interactive Swagger UI**: Professional styling and enhanced UX
- **XML Documentation**: Automatic generation from code comments
- **JWT Authentication**: Integrated Bearer token testing
- **Comprehensive Metadata**: Version, contact, license information
- **Custom Styling**: Professional appearance with dark mode support

## Technologies Used

- **ASP.NET Core 8.0** - Modern web API framework
- **Entity Framework Core 8.0** - Advanced ORM with LINQ support
- **AutoMapper 12.0** - Object-to-object mapping with conventions
- **ASP.NET Core Identity** - Comprehensive authentication and user management
- **JWT Bearer Authentication** - Secure token-based authentication
- **Swagger/OpenAPI 3.0.1** - Interactive API documentation
- **Serilog** - Structured logging with multiple sinks
- **SQL Server** - Enterprise-grade database provider

## Security Features

- **?? JWT Authentication**: Secure token-based authentication with configurable expiry
- **?? Role-based Authorization**: Fine-grained access control based on user roles
- **?? Password Security**: Strong password requirements and secure hashing
- **?? CORS Configuration**: Cross-origin request handling with environment-specific policies
- **??? Security Headers**: Comprehensive protection (XSS, CSP, HSTS, etc.)
- **? Input Validation**: Robust request validation and sanitization
- **?? Request Logging**: Detailed audit trail for security monitoring

## Enhanced Troubleshooting

### ?? Common Issues

#### ? OpenAPI Version Issue - FIXED
**Problem**: "Unable to render this definition - Invalid version field"
**Solution**: 
- Updated to OpenAPI 3.0.1 specification with explicit version
- Enhanced API metadata with comprehensive information
- Automatic XML documentation generation

#### ? Swagger Not Loading - FIXED
**Problem**: "No webpage was found for the web address: https://localhost:7232/swagger"
**Solution**: 
- Properly configured Swagger routing at `/swagger` endpoint
- Enhanced UI with custom styling and improved functionality
- Environment-specific configuration handling

#### Database Connection Issues
**Problem**: Database-related errors on startup
**Solutions**:
- Ensure SQL Server LocalDB is installed and running
- Verify connection string in `appsettings.Development.json`
- Automatic database migration and seeding in Development
- Run manual migrations: `dotnet ef database update`

#### HTTPS Certificate Issues
**Problem**: Certificate trust issues in development
**Solution**:
- Run: `dotnet dev-certs https --trust`
- Restart browser after trusting the certificate
- Enhanced security headers for production readiness

## API Usage Examples

### ?? Authentication
```bash
# Login with enhanced response
POST https://localhost:7232/api/auth/login
Content-Type: application/json

{
  "username": "admin@company.com",
  "password": "SecurePassword123",
  "rememberMe": false
}

# Response includes JWT token and user information
{
  "isSuccess": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "abc123...",
    "expiresAt": "2024-01-16T12:00:00Z",
    "user": { ... }
  }
}

# Get current user (requires Bearer token)
GET https://localhost:7232/api/auth/me
Authorization: Bearer {your-jwt-token}
```

### ?? Employee Management
```bash
# Get employees with enhanced pagination
GET https://localhost:7232/api/employees?pageNumber=1&pageSize=10

# Response with comprehensive pagination metadata
{
  "isSuccess": true,
  "data": {
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}

# Create new employee with validation
POST https://localhost:7232/api/employees
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "employeeCode": "EMP001",
  "fullName": "John Doe",
  "dateOfBirth": "1990-01-01",
  "email": "john.doe@company.com",
  "phoneNumber": "+1234567890",
  "departmentId": 1,
  "workPositionId": 1
}
```

### ?? Dashboard Data
```bash
# Get comprehensive dashboard statistics
GET https://localhost:7232/api/dashboard/statistics
Authorization: Bearer {your-jwt-token}

# Enhanced response with detailed metrics
{
  "isSuccess": true,
  "data": {
    "totalEmployees": 150,
    "totalDepartments": 8,
    "todayAttendance": 142,
    "recentEmployees": [...],
    "departmentStats": [...],
    "attendanceRate": 94.7
  }
}

# Get real-time attendance data
GET https://localhost:7232/api/dashboard/attendance-today
Authorization: Bearer {your-jwt-token}
```

## Development Setup

### Prerequisites
- **.NET 8 SDK** - Latest LTS version
- **SQL Server LocalDB** - For development database
- **Visual Studio 2022** or **VS Code** - IDE with C# support
- **Git** - Version control
- **Postman** (Optional) - API testing tool

### Getting Started
1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SAT.BE
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database** (automatic in Development)
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Enhanced Swagger UI**
   - Open: `https://localhost:7232/swagger`
   - Enjoy the professional API documentation experience!

### Environment Configuration
- **Development**: Uses `appsettings.Development.json` with extended JWT expiry
- **Production**: Uses `appsettings.json` with secure defaults
- **Database**: Automatic creation, migration, and seeding in Development
- **Logging**: Console and file logging with structured data
- **Security**: Environment-specific CORS and security policies

### ?? Enhanced Swagger Features
- **Professional Styling**: Custom CSS for improved appearance
- **Interactive Testing**: Try-it-out enabled by default
- **JWT Integration**: Built-in authentication testing
- **Dark Mode**: Automatic theme detection
- **Enhanced Navigation**: Organized endpoints with filtering
- **Performance Monitoring**: Request duration display
- **Deep Linking**: Shareable endpoint URLs

## Integration Options

### ?? Frontend Integration
- **React/Angular/Vue**: Use the OpenAPI spec for client generation
- **Mobile Apps**: REST API with JWT authentication
- **Postman**: Import OpenAPI spec for testing collections
- **API Clients**: Auto-generate clients from OpenAPI specification

### ?? Monitoring & Analytics
- **Application Insights**: Performance and usage monitoring
- **Serilog Sinks**: Structured logging to multiple destinations
- **Health Checks**: Built-in health monitoring endpoints
- **Metrics**: Request/response metrics and performance data

## Next Steps

To complete the API implementation, consider adding:

1. **? Enhanced Authentication**: Advanced JWT features and refresh tokens
2. **?? Permission-based Authorization**: Fine-grained permission system
3. **?? File Upload Endpoints**: Employee photos and document management
4. **?? Reporting System**: Comprehensive attendance and HR reports
5. **?? Notification System**: Email/SMS notifications for schedules
6. **?? Audit Logging**: Comprehensive change tracking system
7. **? Rate Limiting**: API abuse protection and throttling
8. **?? Advanced Health Checks**: Database, external services monitoring
9. **?? Caching Layer**: Redis/Memory caching for performance
10. **?? Unit Tests**: Comprehensive test coverage with mocking

---

## ?? Success! 

The SAT.BE API now provides a **professional-grade REST API** with:

- **? Enhanced Swagger UI** with custom styling
- **?? OpenAPI 3.0.1 compliance** for modern tooling
- **?? Comprehensive security** features
- **?? Rich documentation** with interactive testing
- **?? Production-ready** architecture and patterns

**?? Quick Access**: Start the application and visit `https://localhost:7232/swagger` to experience the enhanced API documentation interface!

**?? Mobile-Ready**: The API is designed for integration with mobile applications, web frontends, and third-party systems through standard REST conventions and OpenAPI specifications.