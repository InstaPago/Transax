using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PaymentRequest.Startup))]
namespace PaymentRequest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
