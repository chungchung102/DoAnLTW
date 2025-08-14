using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CosmeticsShop.Startup))]

namespace CosmeticsShop
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
