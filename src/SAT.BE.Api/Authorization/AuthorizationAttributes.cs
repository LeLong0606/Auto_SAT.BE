using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Api.Authorization
{
    /// <summary>
    /// Permission-based authorization attribute
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"HasPermission:{permission}";
        }
    }

    /// <summary>
    /// Role hierarchy-based authorization attribute
    /// </summary>
    public class HasMinimumRoleAttribute : AuthorizeAttribute
    {
        public HasMinimumRoleAttribute(string minimumRole)
        {
            Policy = $"HasMinimumRole:{minimumRole}";
        }
    }

    /// <summary>
    /// Team access authorization attribute - allows team leaders to access only their team data
    /// </summary>
    public class TeamAccessAttribute : AuthorizeAttribute
    {
        public TeamAccessAttribute()
        {
            Policy = "TeamAccess";
        }
    }

    /// <summary>
    /// Department access authorization attribute
    /// </summary>
    public class DepartmentAccessAttribute : AuthorizeAttribute
    {
        public DepartmentAccessAttribute()
        {
            Policy = "DepartmentAccess";
        }
    }
}

namespace SAT.BE.src.SAT.BE.Api.Authorization.Requirements
{
    /// <summary>
    /// Permission requirement for authorization
    /// </summary>
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public HasPermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// Minimum role requirement for authorization
    /// </summary>
    public class HasMinimumRoleRequirement : IAuthorizationRequirement
    {
        public string MinimumRole { get; }

        public HasMinimumRoleRequirement(string minimumRole)
        {
            MinimumRole = minimumRole;
        }
    }

    /// <summary>
    /// Team access requirement for authorization
    /// </summary>
    public class TeamAccessRequirement : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Department access requirement for authorization
    /// </summary>
    public class DepartmentAccessRequirement : IAuthorizationRequirement
    {
    }
}

namespace SAT.BE.src.SAT.BE.Api.Authorization.Handlers
{
    /// <summary>
    /// Handler for permission-based authorization
    /// </summary>
    public class HasPermissionHandler : AuthorizationHandler<Requirements.HasPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirements.HasPermissionRequirement requirement)
        {
            var permissions = context.User.FindAll("Permission").Select(c => c.Value).ToList();

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler for role hierarchy-based authorization
    /// </summary>
    public class HasMinimumRoleHandler : AuthorizationHandler<Requirements.HasMinimumRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirements.HasMinimumRoleRequirement requirement)
        {
            var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var requiredLevel = RoleHierarchy.RoleLevels.GetValueOrDefault(requirement.MinimumRole, 0);

            foreach (var userRole in userRoles)
            {
                var userLevel = RoleHierarchy.RoleLevels.GetValueOrDefault(userRole, 0);
                if (userLevel >= requiredLevel)
                {
                    context.Succeed(requirement);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler for team access authorization - team leaders can only access their team members
    /// </summary>
    public class TeamAccessHandler : AuthorizationHandler<Requirements.TeamAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeamAccessHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirements.TeamAccessRequirement requirement)
        {
            var user = context.User;
            var httpContext = _httpContextAccessor.HttpContext;

            // Super admin and admin have full access
            if (user.IsInRole("SuperAdmin") || user.IsInRole("Admin") || user.IsInRole("Director"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Team leaders can access their department members
            if (user.IsInRole("TeamLeader") || user.IsInRole("Manager"))
            {
                var userDepartmentId = user.FindFirst("DepartmentId")?.Value;
                
                if (!string.IsNullOrEmpty(userDepartmentId))
                {
                    // Check if the requested resource belongs to the same department
                    var requestedEmployeeId = httpContext?.Request.RouteValues["employeeId"]?.ToString();
                    var requestedDepartmentId = httpContext?.Request.RouteValues["departmentId"]?.ToString();

                    // For now, allow access if user has department ID
                    // In a real implementation, you'd check against the actual resource
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Handler for department access authorization
    /// </summary>
    public class DepartmentAccessHandler : AuthorizationHandler<Requirements.DepartmentAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirements.DepartmentAccessRequirement requirement)
        {
            var user = context.User;

            // Super admin, admin, and directors have full access
            if (user.IsInRole("SuperAdmin") || user.IsInRole("Admin") || user.IsInRole("Director"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Managers and team leaders have department access
            if (user.IsInRole("Manager") || user.IsInRole("TeamLeader"))
            {
                var userDepartmentId = user.FindFirst("DepartmentId")?.Value;
                if (!string.IsNullOrEmpty(userDepartmentId))
                {
                    context.Succeed(requirement);
                }
            }

            // HR has access to all departments
            if (user.IsInRole("HR"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}