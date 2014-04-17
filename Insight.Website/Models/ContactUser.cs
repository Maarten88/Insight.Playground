using Insight.Database;
using Microsoft.AspNet.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Insight.Website.Models
{
    // public class ApplicationUser: 
    public class ContactUser : IUser<int>
    {
        public ContactUser()
        {
            this.IsDisabled = false;
            this.IsEmailConfirmed = false;
        }

        public ContactUser(int id) 
        {
            this.IsDisabled = false;
            this.IsEmailConfirmed = false;
            this.Id = id;
        }

        public ContactUser(string userName)
        {
            this.IsDisabled = false;
            this.IsEmailConfirmed = false;
            this.UserName = userName;
        }
        public ContactUser(int id, string userName)
        {
            this.IsDisabled = false;
            this.IsEmailConfirmed = false;
            this.Id = id;
            this.UserName = userName;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ContactUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public int Id { get; private set; }

        [Column("MailAddress")]
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }

    }
}
