using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security;

[assembly: OwinStartup(typeof(EntrevistiaWEB.Startup))]

namespace EntrevistiaWEB
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Use a dedicated external cookie so external providers (Google) write the
            // external identity to the ExternalCookie. Then in the callback we read
            // that cookie, create an application identity and sign-in the user.
            app.SetDefaultSignInAsAuthenticationType("ExternalCookie");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = AuthenticationMode.Passive
            });

            // Application cookie (what the app uses to keep the user signed in)
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Home/Login")
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "35229897295-tgio1666kqtmdcggm3vasshdukbr6cf7.apps.googleusercontent.com",
                ClientSecret = "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",
                CallbackPath = new PathString("/signin-google") 



            });
        }
    }
}