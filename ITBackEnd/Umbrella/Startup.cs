using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Umbrella.Startup))]
namespace Umbrella
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
