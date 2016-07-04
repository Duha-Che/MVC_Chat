using DAL;
using Microsoft.AspNet.Identity;

namespace MVC.Chart.CommonData
{
    public class UserPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return DbUser.HashPassword(password).ToString();
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            ulong hash;
            if (!ulong.TryParse(hashedPassword, out hash))
                return PasswordVerificationResult.SuccessRehashNeeded;

            return hash == DbUser.HashPassword(providedPassword) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed; 
        }
    }
}