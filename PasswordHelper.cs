using System.Security.Cryptography;
using System.Text;

namespace MiniB2B.Web.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(plainPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLower(); 
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHashOrPlain)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHashOrPlain))
                return false;

            var enteredHash = HashPassword(enteredPassword);
            if (enteredHash.Equals(storedHashOrPlain, StringComparison.OrdinalIgnoreCase))
                return true;

            if (enteredPassword == storedHashOrPlain)
                return true;

            return false;
        }
    }
}