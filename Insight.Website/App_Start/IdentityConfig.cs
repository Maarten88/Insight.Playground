using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Insight.Website.Models;
using Insight.Website.Identity.Insight;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace Insight.Website
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ContactUserManager : UserManager<ContactUser, int>
    {
        public ContactUserManager(IUserStore<ContactUser, int> store)
            : base(store)
        {
        }

        public static ContactUserManager Create(IdentityFactoryOptions<ContactUserManager> options, IOwinContext context) 
        {
            var manager = new ContactUserManager(context.Get<ContactUserStore>());
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ContactUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            manager.ClaimsIdentityFactory = new ContactClaimsIdentityFactory();

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ContactUser, int>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ContactUser, int>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is: {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ContactUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class ContactClaimsIdentityFactory : ClaimsIdentityFactory<ContactUser, int>
    {
        public override async Task<ClaimsIdentity> CreateAsync(UserManager<ContactUser, int> manager, ContactUser user, string authenticationType)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if ((object)user == null)
                throw new ArgumentNullException("user");

            if (user.Id == 0) // happens after create new user
                user = await manager.FindByNameAsync(user.UserName); // userName must be unique for this to work

            ClaimsIdentity id = new ClaimsIdentity(authenticationType, this.UserNameClaimType, this.RoleClaimType);
            id.AddClaim(new Claim(this.UserIdClaimType, this.ConvertIdToString(user.Id), "http://www.w3.org/2001/XMLSchema#string"));
            id.AddClaim(new Claim(this.UserNameClaimType, user.UserName, "http://www.w3.org/2001/XMLSchema#string"));
            id.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"));

            if (manager.SupportsUserRole)
            {
                IList<string> roles = await manager.GetRolesAsync(user.Id).ConfigureAwait(false);
                foreach (string str in (IEnumerable<string>)roles)
                    id.AddClaim(new Claim(this.RoleClaimType, str, "http://www.w3.org/2001/XMLSchema#string"));
            }
            if (manager.SupportsUserClaim)
                id.AddClaims((IEnumerable<Claim>)await manager.GetClaimsAsync(user.Id).ConfigureAwait(false));
            return id;
        }
    }

}
