namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public static class PermissionConstants
    {
        // Employee Permissions
        public const string EMPLOYEE_VIEW = "EMPLOYEE_VIEW";
        public const string EMPLOYEE_CREATE = "EMPLOYEE_CREATE";
        public const string EMPLOYEE_UPDATE = "EMPLOYEE_UPDATE";
        public const string EMPLOYEE_DELETE = "EMPLOYEE_DELETE";
        public const string EMPLOYEE_VIEW_TEAM = "EMPLOYEE_VIEW_TEAM"; // Team leaders can only view their team
        
        // Department Permissions
        public const string DEPARTMENT_VIEW = "DEPARTMENT_VIEW";
        public const string DEPARTMENT_CREATE = "DEPARTMENT_CREATE";
        public const string DEPARTMENT_UPDATE = "DEPARTMENT_UPDATE";
        public const string DEPARTMENT_DELETE = "DEPARTMENT_DELETE";
        public const string DEPARTMENT_MANAGE_ALL = "DEPARTMENT_MANAGE_ALL";
        
        // Scheduling Permissions
        public const string SCHEDULE_VIEW = "SCHEDULE_VIEW";
        public const string SCHEDULE_CREATE = "SCHEDULE_CREATE";
        public const string SCHEDULE_UPDATE = "SCHEDULE_UPDATE";
        public const string SCHEDULE_DELETE = "SCHEDULE_DELETE";
        public const string SCHEDULE_CREATE_TEAM = "SCHEDULE_CREATE_TEAM"; // Team leaders can create schedules for their team
        public const string SCHEDULE_CREATE_ALL = "SCHEDULE_CREATE_ALL"; // Directors can create schedules for all
        
        // System Permissions
        public const string USER_MANAGEMENT = "USER_MANAGEMENT";
        public const string ROLE_MANAGEMENT = "ROLE_MANAGEMENT";
        public const string SYSTEM_CONFIGURATION = "SYSTEM_CONFIGURATION";
        
        // Attendance Permissions
        public const string ATTENDANCE_VIEW = "ATTENDANCE_VIEW";
        public const string ATTENDANCE_CREATE = "ATTENDANCE_CREATE";
        public const string ATTENDANCE_UPDATE = "ATTENDANCE_UPDATE";
        public const string ATTENDANCE_VIEW_TEAM = "ATTENDANCE_VIEW_TEAM"; // Team leaders can view their team's attendance
        public const string ATTENDANCE_VIEW_ALL = "ATTENDANCE_VIEW_ALL"; // Directors/Admins can view all attendance
        
        // Report Permissions
        public const string REPORT_VIEW = "REPORT_VIEW";
        public const string REPORT_EXPORT = "REPORT_EXPORT";
        public const string REPORT_VIEW_TEAM = "REPORT_VIEW_TEAM"; // Team leaders can view their team's reports
        public const string REPORT_VIEW_ALL = "REPORT_VIEW_ALL"; // Directors/Admins can view all reports
    }
    
    public static class PermissionCategories
    {
        public const string HR = "HR";
        public const string SCHEDULING = "Scheduling";
        public const string ATTENDANCE = "Attendance";
        public const string SYSTEM = "System";
        public const string REPORTING = "Reporting";
    }

    public static class RoleHierarchy
    {
        public const string SUPER_ADMIN = "SuperAdmin";
        public const string ADMIN = "Admin";
        public const string DIRECTOR = "Director";
        public const string MANAGER = "Manager";
        public const string TEAM_LEADER = "TeamLeader";
        public const string HR = "HR";
        public const string EMPLOYEE = "Employee";
        public const string USER = "User";

        // Role levels for hierarchy checking
        public static readonly Dictionary<string, int> RoleLevels = new()
        {
            { SUPER_ADMIN, 10 },
            { ADMIN, 9 },
            { DIRECTOR, 8 },
            { MANAGER, 7 },
            { HR, 6 },
            { TEAM_LEADER, 5 },
            { EMPLOYEE, 4 },
            { USER, 1 }
        };
    }
}