using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC.Chart.Startup))]
namespace MVC.Chart
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            app.MapSignalR();
        }
    }
}
