using StudentManagement.Models;

namespace StudentManagement.Helpers
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }
        public static Student CurrentStudent { get; set; }
        public static Teacher CurrentTeacher { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsTeacher => CurrentUser?.Role == UserRole.Teacher;
        public static bool IsStudent => CurrentUser?.Role == UserRole.Student;

        public static void Logout()
        {
            CurrentUser = null;
            CurrentStudent = null;
            CurrentTeacher = null;
        }
    }
}
