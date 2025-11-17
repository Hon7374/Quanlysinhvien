using System;

namespace StudentManagement.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public string Description { get; set; }
        public int? TeacherId { get; set; }
        public string Semester { get; set; }
        public int AcademicYear { get; set; }
        public int MaxStudents { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public Teacher Teacher { get; set; }

        public Course()
        {
            IsActive = true;
            MaxStudents = 50;
        }
    }
}
