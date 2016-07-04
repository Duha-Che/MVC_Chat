using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MVC.Chart.CommonData;
using MVC.Chart.Models;

namespace MVC.Chart
{
    // Настройка диспетчера пользователей приложения. UserManager определяется в ASP.NET Identity и используется приложением.
    public class ApplicationUserManager : UserManager<ApplicationUser, Guid>
    {
        public const int PasswordRequiredLength = 6;

        public ApplicationUserManager(IUserStore<ApplicationUser, Guid> store)
            : base(store)
        {
        }

        private UserRepositoryAdapter _userRepoAdapter;

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            //var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            var userRepository = new UserRepositoryAdapter();
            var manager = new ApplicationUserManager(userRepository);
            // Настройка логики проверки имен пользователей
            manager.UserValidator = new UserValidator(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            manager._userRepoAdapter = userRepository;


            // Настройка логики проверки паролей
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = PasswordRequiredLength,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Настройка параметров блокировки по умолчанию
            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser, Guid>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            manager.PasswordHasher = new UserPasswordHasher();
            return manager;
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            return _userRepoAdapter.CreateAsync(user, password);
        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return _userRepoAdapter.CheckPasswordAsync(user, password);
        }

        public override Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string authenticationType)
        {
            return base.CreateIdentityAsync( user, authenticationType);
        }
    }

    public class UserValidator : UserValidator<ApplicationUser, Guid>
    {
        private UserManager<ApplicationUser, Guid> _manager;

        public UserValidator(UserManager<ApplicationUser, Guid> manager) 
            : base(manager)
        {
            RequireUniqueEmail = false;
            _manager = manager;
        }

        public override Task<IdentityResult> ValidateAsync(ApplicationUser item)
        {
            return _manager
                .CheckPasswordAsync(item, item.PasswordHash)
                .ContinueWith( t => t.Result ? IdentityResult.Success : IdentityResult.Failed() );              
        }
    }

    // Настройка диспетчера входа для приложения.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, Guid>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync( UserManager);
        }

        public override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var user = new ApplicationUser()
            {
                UserName = userName,
                PasswordHash = password
            };

            Func<SignInStatus> onSuccess = 
                () => 
                    {
                        SignInAsync(user, isPersistent, true).Wait();
                        return SignInStatus.Success;
                    };
            return UserManager
                .CheckPasswordAsync(user, password)
                .ContinueWith(t => t.Result ? onSuccess(): SignInStatus.Failure )
                //.ContinueWith(t => t.Result ? SignInStatus.Success : SignInStatus.Failure)
                ;
        }

      
        public override Guid ConvertIdFromString(string id)
        {
            Guid result;
            if (!Guid.TryParse(id, out result))
            {
                throw new FormatException( String.Format( "[ApplicationSignInManager] Can't pase {0} as Guid", id ) );
            }
            return result;
        }

        public override string ConvertIdToString(Guid id)
        {
           return id.ToString();
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
