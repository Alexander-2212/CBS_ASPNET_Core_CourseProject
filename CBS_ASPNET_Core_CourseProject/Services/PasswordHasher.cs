using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Text;
using CBS_ASPNET_Core_CourseProject.Entities;

namespace CBS_ASPNET_Core_CourseProject.Services
{
    public class PasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            var providedPasswordHash = HashPassword(user, providedPassword);
            if (hashedPassword == providedPasswordHash)
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }
    }
}
