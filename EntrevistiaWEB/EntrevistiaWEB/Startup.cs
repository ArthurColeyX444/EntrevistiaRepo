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
            app.SetDefaultSignInAsAuthenticationType("ApplicationCookie");

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


  Mensaje =
  Origen =
  Seguimiento de la pila:
  at eval(eval at<anonymous>(eval at<anonymous>(eval at<anonymous>(eval at<anonymous>(:2:4011)))), < anonymous >:1:102)
    at iA(eval at<anonymous>(:2:4011), < anonymous >:1438:21388)
    at V.eval[as G](eval at<anonymous>(:2:4011), < anonymous >:1438:54857)
    at gJ(eval at<anonymous>(:2:4011), < anonymous >:1438:37544)
    at JA(eval at<anonymous>(:2:4011), < anonymous >:1438:12971)
    at rJ(eval at<anonymous>(:2:4011), < anonymous >:1438:9549)
    at y0(eval at<anonymous>(:2:4011), < anonymous >:1438:36882)
    at new V(eval at<anonymous>(:2:4011), < anonymous >:1438:27806)
    at M(eval at<anonymous>(:2:4011), < anonymous >:1438:4234)
    at ku(eval at<anonymous>(:2:4011), < anonymous >:1438:36980)

            });
        }
    }
}