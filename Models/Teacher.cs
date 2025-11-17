using System;

namespace StudentManagement.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public int UserId { get; set; }
        public string TeacherCode { get; set; }
        public string Department { get; set; }
        public string Degree { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }

        // Navigation property
        public User User { get; set; }

        public Teacher()
        {
            HireDate = DateTime.Now;
        }
    }
}
