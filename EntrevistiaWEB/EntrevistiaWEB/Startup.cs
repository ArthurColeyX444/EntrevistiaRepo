using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;

[assembly: OwinStartup(typeof(EntrevistiaWEB.Startup))]

namespace EntrevistiaWEB
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType("ApplicationCookie");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Home/Login")
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "TU_CLIENT_ID",
                ClientSecret = "TU_CLIENT_SECRET"
            });
        }
    }
}