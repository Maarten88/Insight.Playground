using Insight.Website.Models;
using Insight.Database;
using Insight.Database.Reliable;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Insight.Website.Identity.Insight
{
    public class ContactUserStore : IUserStore<ContactUser, int>,
                                    IUserEmailStore<ContactUser, int>,
                                    IUserClaimStore<ContactUser, int>,
                                    IUserRoleStore<ContactUser, int>,
                                    IUserPasswordStore<ContactUser, int>,
                                    IUserLoginStore<ContactUser, int>,
                                    IDisposable
    {
        private readonly string ProviderNameKey = "InsightDbProvider";

        private readonly DbConnection _connection;

        public ContactUserStore(DbConnection connection)
        {
            _connection = connection;
        }

        public static ContactUserStore Create(IdentityFactoryOptions<ContactUserStore> options, IOwinContext context)
        {
            return new ContactUserStore(context.Get<DbConnection>());
        }

        public Task CreateAsync(ContactUser user)
        {
            return _connection.SingleAsync<ContactUser>("InsertContact", user);
        }

        public Task DeleteAsync(ContactUser user)
        {
            return _connection.ExecuteAsync("DeleteContact", new { Id = user.Id });
        }

        public Task<ContactUser> FindByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Zero or empty argument: userId");
            }
            return _connection.SingleAsync<ContactUser>("GetContactById", new { Id = userId }); 
        }

        public Task<ContactUser> FindByNameAsync(string userName)
        {
            return _connection.SingleAsync<ContactUser>("GetContactByEmail", new { Email = userName }); 
        }

        public Task UpdateAsync(ContactUser user)
        {
            return _connection.ExecuteAsync("UpdateContact", user);
        }

        public Task AddClaimAsync(ContactUser user, Claim claim)
        {
            throw new NotImplementedException("Add Claim Not Implement");
        }

        public async Task<IList<Claim>> GetClaimsAsync(ContactUser user)
        {
            var result = await _connection.QueryAsync<IList<Claim>>("GetClaims", new { Id = user.Id }, reader =>
            {
                var claims = new List<Claim>();
                while (reader.Read())
                {
                    claims.Add(new Claim(reader["ClaimType"].ToString(), reader["ClaimValue"].ToString(), ProviderNameKey));
                }
                return claims;
            });
            return result;
        }

        public Task RemoveClaimAsync(ContactUser user, Claim claim)
        {
            throw new NotImplementedException("Remove Claim Not Implement");
        }

        public Task AddToRoleAsync(ContactUser user, string role)
        {
            return _connection.ExecuteAsync("InsertContactRole", new { ContactId = user.Id, RoleName = role }); 
        }

        public Task<IList<string>> GetRolesAsync(ContactUser user)
        {
            return _connection.QueryAsync<string>("GetContactRolesByContactId", new { Id = user.Id }); 
        }

        public Task<bool> IsInRoleAsync(ContactUser user, string role)
        {
            return _connection.ExecuteScalarAsync<bool>("IsContactInRole", new { ContactId = user.Id, RoleName = role });
        }

        public Task RemoveFromRoleAsync(ContactUser user, string role)
        {
            return _connection.ExecuteAsync("DeleteContactRoleByContactIdAndRole", new { ContactId = user.Id, RoleName = role }); 
        }

        public Task<string> GetPasswordHashAsync(ContactUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ContactUser user)
        {
            return Task.FromResult(!String.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(ContactUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task AddLoginAsync(ContactUser user, UserLoginInfo login)
        {
            return _connection.ExecuteAsync("InsertContactLogin", new { ContactId = user.Id, LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey });
        }

        public Task<ContactUser> FindAsync(UserLoginInfo login)
        {
            var result = _connection.SingleAsync<ContactUser>("FindContactFromExternalLogin", new { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey });
            return result;
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ContactUser user)
        {
            var result = _connection.QueryAsync<IList<UserLoginInfo>>("GetContactLogins", new { ContactId = user.Id }, reader =>
            {
                var loginInfo = new List<UserLoginInfo>();
                while (reader.Read())
                {
                    loginInfo.Add(new UserLoginInfo(reader["LoginProvider"].ToString(), reader["ProviderKey"].ToString()));
                }
                return loginInfo;
            });
            return result;
        }

        public Task RemoveLoginAsync(ContactUser user, UserLoginInfo login)
        {
            return _connection.ExecuteAsync("DeleteContactLogin", new { ContactId = user.Id, LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey });
        }

        public Task<ContactUser> FindByEmailAsync(string email)
        {
            var result = _connection.SingleAsync<ContactUser>("GetContactByEmail", new { Email = email });
            return result;
        }

        public Task<string> GetEmailAsync(ContactUser user)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> GetEmailConfirmedAsync(ContactUser user)
        {
            return Task.FromResult(user.IsEmailConfirmed);
        }

        public Task SetEmailAsync(ContactUser user, string email)
        {
            user.UserName = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(ContactUser user, bool confirmed)
        {
            user.IsEmailConfirmed = confirmed;
            return Task.FromResult(0);
        }
        protected void Dispose(bool disposing)
        {
            if (disposing && _connection != null)
            {
                _connection.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}