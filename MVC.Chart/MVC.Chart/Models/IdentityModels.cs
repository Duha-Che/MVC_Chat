using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using DAL;
using Microsoft.AspNet.Identity;

namespace MVC.Chart.Models
{
    // Чтобы добавить данные профиля для пользователя, можно добавить дополнительные свойства в класс ApplicationUser. Дополнительные сведения см. по адресу: http://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IUser<Guid>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser,Guid> manager)
        {
            if (Id.Equals(Guid.Empty))
            {
                var user = UserRepository.Instance.FindByName(UserName);
                Id = user == null ? Guid.Empty : user.Id;
            }
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }

        public static implicit operator DbUser(ApplicationUser user)
        {
            var node = new XElement(DbUser.tagRoot, new XAttribute(DbUser.tagId, user.Id),
                        new XAttribute(DbUser.tagName, user.UserName),
                        new XAttribute(DbUser.tagPassword, DbUser.HashPassword( user.PasswordHash) ));

            return DbUser.DeserializeFromXml(node);
        }

        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }

        internal static Guid GetIdFor(ClaimsIdentity claim)
        {
            if (claim.IsAuthenticated)
            {
                return claim.GetUserIdAsGuid();
                //var user = UserRepository.Instance.FindByName(claim.Name);

                //if (user != null)
                //    return user.Id;
            }

            return Guid.Empty;
        }
    }

    public static class IdentityExtensions
    {

        public static Guid GetUserIdAsGuid(this IIdentity identity)
        {
            Guid result;
            var key = identity.GetUserId();
            if (Guid.TryParse(key, out result) && result != Guid.Empty )
                return result;

            var user = UserRepository.Instance.FindByName(identity.Name);

            return user == null ? Guid.Empty : user.Id;
        }

    }
}