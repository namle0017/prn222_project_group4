namespace FapWeb.Infrastructure
{
    public static class AppRoles
    {
        public const string Admin = "ADMIN";
        public const string Teacher = "TEACHER";
        public const string Parent = "PARENT";
        public const string Student = "STUDENT";

        public static bool IsAdmin(string? roleName)
        {
            return string.Equals(roleName, Admin, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTeacher(string? roleName)
        {
            return string.Equals(roleName, Teacher, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsParent(string? roleName)
        {
            return string.Equals(roleName, Parent, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsStudent(string? roleName)
        {
            return string.Equals(roleName, Student, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsStaff(string? roleName)
        {
            return IsAdmin(roleName) || IsTeacher(roleName);
        }
    }
}
