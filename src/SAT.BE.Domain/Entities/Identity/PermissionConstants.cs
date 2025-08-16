namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public static class PermissionConstants
    {
        // Employee Permissions
        public const string EMPLOYEE_VIEW = "EMPLOYEE_VIEW";
        public const string EMPLOYEE_CREATE = "EMPLOYEE_CREATE";
        public const string EMPLOYEE_UPDATE = "EMPLOYEE_UPDATE";
        public const string EMPLOYEE_DELETE = "EMPLOYEE_DELETE";
        
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
        
        // System Permissions
        public const string USER_MANAGEMENT = "USER_MANAGEMENT";
        public const string ROLE_MANAGEMENT = "ROLE_MANAGEMENT";
        public const string SYSTEM_CONFIGURATION = "SYSTEM_CONFIGURATION";
        
        // Attendance Permissions
        public const string ATTENDANCE_VIEW = "ATTENDANCE_VIEW";
        public const string ATTENDANCE_CREATE = "ATTENDANCE_CREATE";
        public const string ATTENDANCE_UPDATE = "ATTENDANCE_UPDATE";
        
        // Report Permissions
        public const string REPORT_VIEW = "REPORT_VIEW";
        public const string REPORT_EXPORT = "REPORT_EXPORT";
    }
    
    public static class PermissionCategories
    {
        public const string HR = "HR";
        public const string SCHEDULING = "Scheduling";
        public const string ATTENDANCE = "Attendance";
        public const string SYSTEM = "System";
        public const string REPORTING = "Reporting";
    }
}