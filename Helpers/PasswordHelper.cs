using System;
using System.Security.Cryptography;
using System.Text;

namespace StudentManagement.Helpers
{
    public static class PasswordHelper
    {
        // PBKDF2 (Rfc2898) with random salt. Stored format: iterations:salt:hash (all Base64)
        private const int Pbkdf2Iter = 100_000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public static string HashPassword(string password)
        {
            // Generate salt
            byte[] salt = new byte[SaltBytes];
            RandomNumberGenerator.Fill(salt);

            // Derive key
            using (var derive = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iter, HashAlgorithmName.SHA256))
            {
                byte[] hash = derive.GetBytes(HashBytes);
                // Store as iterations:salt:hash in Base64
                return $"{Pbkdf2Iter}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            try
            {
                // Backwards compatibility:
                // If storedHash contains ':' we assume PBKDF2 format
                if (storedHash.Contains(':'))
                {
                    var parts = storedHash.Split(':');
                    if (parts.Length != 3) return false;
                    int iterations = int.Parse(parts[0]);
                    byte[] salt = Convert.FromBase64String(parts[1]);
                    byte[] stored = Convert.FromBase64String(parts[2]);

                    using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                    {
                        byte[] computed = derive.GetBytes(stored.Length);
                        return CryptographicOperations.FixedTimeEquals(computed, stored);
                    }
                }

                // If storedHash looks like a SHA256 hex string (64 hex chars), compare with SHA256
                if (storedHash.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(storedHash, "^[a-fA-F0-9]{64}$"))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                        var sb = new StringBuilder();
                        foreach (byte b in bytes)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        return StringComparer.OrdinalIgnoreCase.Compare(sb.ToString(), storedHash) == 0;
                    }
                }

                // Fallback: plaintext (unsafe). Compare directly (compatibility only).
                return password == storedHash;
            }
            catch
            {
                // Any parse or other error -> treat as verification failure
                return false;
            }
        }
    }
}
