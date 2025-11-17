using System;

namespace StudentManagement.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public string StudentCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Class { get; set; }
        public string Major { get; set; }
        public int AcademicYear { get; set; }
        public decimal GPA { get; set; }

        // Navigation property
        public User User { get; set; }

        public Student()
        {
            GPA = 0.0m;
        }
    }
}
