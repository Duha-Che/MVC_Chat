using System;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNet.Identity;
using MVC.Chart.Models;

namespace MVC.Chart.CommonData
{
    public class UserRepositoryAdapter : IUserStore<ApplicationUser, Guid>, IUserPasswordStore<ApplicationUser, Guid>
    {
        UserRepository _repo;

        public UserRepositoryAdapter()
        {
            _repo = UserRepository.Instance;

        }

        public Task CreateAsync(ApplicationUser user)
        {
            var task = new Task(async () => await CreateAsync(user, user.PasswordHash) );
            task.Start();
            return task;
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            var task = new Task(() => _repo.DeleteUser( user.Id ));
            task.Start();
            return task;
        }

        public void Dispose()
        {
            //_base.Dispose();
        }

        public Task<ApplicationUser> FindByIdAsync(Guid userId)
        {
            var task = new Task<ApplicationUser>(() =>
            {
                var user = _repo.FindByKey(userId);
                return user == null ? null : new ApplicationUser() { Id = user.Id, UserName = user.Name, PasswordHash = user.Password.ToString() };
            });
            task.Start();
            return task;
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            var task = new Task<ApplicationUser>(() =>
            {
                var user = _repo.FindByName(userName);
                return user == null ? null : new ApplicationUser() { Id = user.Id, UserName = user.Name, PasswordHash = user.Password.ToString() };
            });
            task.Start();
            return task;
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            var task = new Task(() => _repo.UpdateUser(user));
            task.Start();
            return task;

        }

        internal Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
             var task = new Task<IdentityResult>(() => 
                {
                    var dbUser = DbUser.NewUser(user.UserName, password);
                    try
                    {
                        _repo.AddUser(dbUser);
                        return IdentityResult.Success;
                    }
                    catch (Exception e)
                    {
                        return IdentityResult.Failed(e.Message);
                    }
                });
            task.Start();
            return task;
        }

        internal Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var task = new Task<bool>(() =>
            {
                return _repo.CheckPassword(user.UserName, password);
            });
            task.Start();
            return task;
        }

        Task IUserPasswordStore<ApplicationUser, Guid>.SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            var task = new Task(() =>
            {
                var dbUser = _repo.FindByKey(user.Id) ?? _repo.FindByName( user.UserName );
                if (dbUser == null)
                    throw new Exception(
                        String.Format("[UserRepositoryAdapter.SetPasswordHashAsync] User is not found in db ( id : '{0}', Name : '{1}'", user.Id, user.UserName));
                dbUser.SetPasswordHash(passwordHash);
                _repo.UpdateUser(dbUser);
                user.PasswordHash = dbUser.Password.ToString();
            });
            task.Start();
            return task;
        }

        Task<string> IUserPasswordStore<ApplicationUser, Guid>.GetPasswordHashAsync(ApplicationUser user)
        {
            var task = new Task<string>(() =>
            {
                var dbUser = _repo.FindByKey(user.Id) ?? _repo.FindByName(user.UserName);
                if (dbUser == null)
                    throw new Exception(
                        String.Format("[UserRepositoryAdapter.GetPasswordHashAsync] User is not found in db ( id : '{0}', Name : '{1}'", user.Id, user.UserName));
                return dbUser.Password.ToString();
            });
            task.Start();
            return task;
        }

        Task<bool> IUserPasswordStore<ApplicationUser, Guid>.HasPasswordAsync(ApplicationUser user)
        {
            var task = new Task<bool>(() =>
            {
                var dbUser = _repo.FindByKey(user.Id) ?? _repo.FindByName(user.UserName);
                if (dbUser == null)
                    throw new Exception(
                        String.Format("[UserRepositoryAdapter.HasPasswordAsync] User is not found in db ( id : '{0}', Name : '{1}'", user.Id, user.UserName));
                return true;
            });
            task.Start();
            return task;
        }
    }
}