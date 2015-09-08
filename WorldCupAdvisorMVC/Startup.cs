using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorldCupAdvisorMVC.Startup))]
namespace WorldCupAdvisorMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
