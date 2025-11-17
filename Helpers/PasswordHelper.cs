using System;
using System.Security.Cryptography;
using System.Text;

namespace StudentManagement.Helpers
{
    public static class PasswordHelper
    {
        // Hash password using SHA256 (for production, use BCrypt or PBKDF2)
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Verify password
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }

        // For demo purposes, simple comparison (replace with hash in production)
        public static bool SimpleVerify(string inputPassword, string storedPassword)
        {
            return inputPassword == storedPassword;
        }
    }
}
