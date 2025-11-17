using System;

namespace StudentManagement.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } // "Enrolled", "Completed", "Dropped"

        // Navigation properties
        public Student Student { get; set; }
        public Course Course { get; set; }

        public Enrollment()
        {
            EnrollmentDate = DateTime.Now;
            Status = "Enrolled";
        }
    }
}
