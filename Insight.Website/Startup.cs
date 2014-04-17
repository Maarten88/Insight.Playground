using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Insight.Website.Startup))]
namespace Insight.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
