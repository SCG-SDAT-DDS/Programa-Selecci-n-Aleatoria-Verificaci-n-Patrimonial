using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Transparencia.Startup))]
namespace Transparencia
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
