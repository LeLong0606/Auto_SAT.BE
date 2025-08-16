namespace SAT.BE.src.SAT.BE.Domain.Entities.Identity
{
    public static class PermissionConstants
    {
        // HR Management Permissions
        public const string EMPLOYEE_VIEW = "EMPLOYEE_VIEW";
        public const string EMPLOYEE_CREATE = "EMPLOYEE_CREATE";
        public const string EMPLOYEE_UPDATE = "EMPLOYEE_UPDATE";
        public const string EMPLOYEE_DELETE = "EMPLOYEE_DELETE";
        public const string EMPLOYEE_MANAGE_ALL = "EMPLOYEE_MANAGE_ALL";

        public const string DEPARTMENT_VIEW = "DEPARTMENT_VIEW";
        public const string DEPARTMENT_CREATE = "DEPARTMENT_CREATE";
        public const string DEPARTMENT_UPDATE = "DEPARTMENT_UPDATE";
        public const string DEPARTMENT_DELETE = "DEPARTMENT_DELETE";
        public const string DEPARTMENT_MANAGE_ALL = "DEPARTMENT_MANAGE_ALL";

        public const string POSITION_VIEW = "POSITION_VIEW";
        public const string POSITION_CREATE = "POSITION_CREATE";
        public const string POSITION_UPDATE = "POSITION_UPDATE";
        public const string POSITION_DELETE = "POSITION_DELETE";

        // Task Management Permissions
        public const string TASK_VIEW = "TASK_VIEW";
        public const string TASK_CREATE = "TASK_CREATE";
        public const string TASK_UPDATE = "TASK_UPDATE";
        public const string TASK_DELETE = "TASK_DELETE";
        public const string TASK_ASSIGN = "TASK_ASSIGN";
        public const string TASK_MANAGE_ALL = "TASK_MANAGE_ALL";

        // Scheduling Management Permissions
        public const string SHIFT_VIEW = "SHIFT_VIEW";
        public const string SHIFT_CREATE = "SHIFT_CREATE";
        public const string SHIFT_UPDATE = "SHIFT_UPDATE";
        public const string SHIFT_DELETE = "SHIFT_DELETE";

        public const string SCHEDULE_VIEW = "SCHEDULE_VIEW";
        public const string SCHEDULE_VIEW_ALL = "SCHEDULE_VIEW_ALL";
        public const string SCHEDULE_CREATE = "SCHEDULE_CREATE";
        public const string SCHEDULE_UPDATE = "SCHEDULE_UPDATE";
        public const string SCHEDULE_DELETE = "SCHEDULE_DELETE";
        public const string SCHEDULE_ASSIGN = "SCHEDULE_ASSIGN";
        public const string SCHEDULE_APPROVE = "SCHEDULE_APPROVE";

        // System Management Permissions
        public const string USER_MANAGEMENT = "USER_MANAGEMENT";
        public const string ROLE_MANAGEMENT = "ROLE_MANAGEMENT";
        public const string PERMISSION_MANAGEMENT = "PERMISSION_MANAGEMENT";
        public const string SYSTEM_SETTINGS = "SYSTEM_SETTINGS";
        public const string AUDIT_LOG_VIEW = "AUDIT_LOG_VIEW";

        // Report Permissions
        public const string REPORT_HR = "REPORT_HR";
        public const string REPORT_SCHEDULING = "REPORT_SCHEDULING";
        public const string REPORT_ATTENDANCE = "REPORT_ATTENDANCE";
        public const string REPORT_EXPORT = "REPORT_EXPORT";

        // Department Level Permissions
        public const string DEPT_EMPLOYEE_VIEW = "DEPT_EMPLOYEE_VIEW";
        public const string DEPT_EMPLOYEE_MANAGE = "DEPT_EMPLOYEE_MANAGE";
        public const string DEPT_SCHEDULE_VIEW = "DEPT_SCHEDULE_VIEW";
        public const string DEPT_SCHEDULE_MANAGE = "DEPT_SCHEDULE_MANAGE";
    }

    public static class RoleConstants
    {
        public const string SUPER_ADMIN = "SuperAdmin";
        public const string ADMIN = "Admin";
        public const string HR_MANAGER = "HRManager";
        public const string DEPARTMENT_MANAGER = "DepartmentManager";
        public const string TEAM_LEADER = "TeamLeader";
        public const string EMPLOYEE = "Employee";
        public const string VIEWER = "Viewer";
    }

    public static class PermissionCategories
    {
        public const string HR = "HR";
        public const string SCHEDULING = "Scheduling";
        public const string TASK_MANAGEMENT = "TaskManagement";
        public const string SYSTEM = "System";
        public const string REPORT = "Report";
        public const string DEPARTMENT = "Department";
    }
}