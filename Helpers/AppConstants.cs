using System;

namespace StudentManagement.Helpers
{
    public static class AppConstants
    {
        // Enrollment status (DB values)
        public const string ENROLLMENT_STATUS_ENROLLED = "Enrolled";
        public const string ENROLLMENT_STATUS_COMPLETED = "Completed";
        public const string ENROLLMENT_STATUS_DROPPED = "Dropped";
        public const string ENROLLMENT_STATUS_CANCELLED = "Cancelled";

        // Payment status (DB values)
        public const string PAYMENT_STATUS_PENDING = "Pending";
        public const string PAYMENT_STATUS_PAID = "Paid";
        public const string PAYMENT_STATUS_UNPAID = "Unpaid";
        public const string PAYMENT_STATUS_FAILED = "Failed";

        // Student status (UI & DB visible values)
        public const string STUDENT_STATUS_ENROLLED = "Đang học";
        public const string STUDENT_STATUS_SUSPEND = "Bảo lưu";
        public const string STUDENT_STATUS_GRADUATED = "Đã tốt nghiệp";
        public const string STUDENT_STATUS_LEFT = "Đã thôi học";

        // User account status
        public const string USER_STATUS_ACTIVE_VN = "Hoạt động";
        public const string USER_STATUS_INACTIVE_VN = "Khóa";

        // Other
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_TEACHER = "Teacher";
        public const string ROLE_STUDENT = "Student";
    }
}
