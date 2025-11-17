using System;

namespace StudentManagement.Models
{
    public enum UserRole
    {
        Admin = 1,
        Teacher = 2,
        Student = 3
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }

        public User()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }
}
