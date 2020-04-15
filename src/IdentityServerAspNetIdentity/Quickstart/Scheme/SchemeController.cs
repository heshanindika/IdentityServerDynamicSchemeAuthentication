using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2.WebSso;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Quickstart.Scheme
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemeController : ControllerBase
    {
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly IServiceProvider serviceProvider;

        public SchemeController(IAuthenticationSchemeProvider schemeProvider, IServiceProvider serviceProvider)
        {
            this.schemeProvider = schemeProvider;
            this.serviceProvider = serviceProvider;
        }

        public IActionResult Remove(string domain, string scheme)
        {
            var schemeName = string.Format("{0}_{1}", domain, scheme);
            //schemeProvider.RemoveScheme(schemeName);
            //oAuthOptionsCache.TryRemove(schemeName);
            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SchemeData schemeData)
        {
            var schemeName = string.Format("{0}_{1}", schemeData.Domain, schemeData.Scheme);
            if(schemeData.Scheme == "Google")
            {
                IOptionsMonitorCache<GoogleOptions> oAuthOptionsCache = serviceProvider.GetRequiredService<IOptionsMonitorCache<GoogleOptions>>();
                OAuthPostConfigureOptions<GoogleOptions, GoogleHandler> oAuthPostConfigureOptions = serviceProvider.GetRequiredService<OAuthPostConfigureOptions<GoogleOptions, GoogleHandler>>();

                if (await schemeProvider.GetSchemeAsync(schemeName) == null)
                {
                    schemeProvider.AddScheme(new AuthenticationScheme(schemeName, schemeData.Scheme, typeof(GoogleHandler)));
                }
                else
                {
                    oAuthOptionsCache.TryRemove(schemeName);
                }
                var options = new GoogleOptions
                {
                    ClientId = "xxxxxxx",
                    ClientSecret = "xxxxxxxxxxxx"
                };
                oAuthPostConfigureOptions.PostConfigure(schemeName, options);
                oAuthOptionsCache.TryAdd(schemeName, options);
            }
            else if (schemeData.Scheme == "Auth0")
            {
                IOptionsMonitorCache<Saml2Options> oAuthOptionsCache = serviceProvider.GetRequiredService<IOptionsMonitorCache<Saml2Options>>();
                PostConfigureSaml2Options oAuthPostConfigureOptions = serviceProvider.GetRequiredService<PostConfigureSaml2Options>();

                if (await schemeProvider.GetSchemeAsync(schemeName) == null)
                {
                    schemeProvider.AddScheme(new AuthenticationScheme(schemeName, schemeData.Scheme, typeof(Saml2Handler)));
                }
                else
                {
                    oAuthOptionsCache.TryRemove(schemeName);
                }
                //urn:ccidentity.auth0.com
                var options = new Saml2Options();
                options.SPOptions.EntityId = new EntityId("https://localhost:44332/auth0");
                options.SPOptions.ModulePath = "/Saml2Auth0";
                options.SPOptions.MinIncomingSigningAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

                var idp = new IdentityProvider(new EntityId("urn:ccidentity.auth0.com"), options.SPOptions)
                {
                    MetadataLocation = "https://xxxxxxxx/samlp/metadata/7HmaqIPuC32Pc95e0clSqN3n3ogzkTkP",
                    LoadMetadata = true,
                    AllowUnsolicitedAuthnResponse = true,
                    Binding = Saml2BindingType.HttpRedirect,
                    SingleSignOnServiceUrl = new Uri("https://xxxxxxx/samlp/7HmaqIPuC32Pc95e0clSqN3n3ogzkTkP"),

                };

                idp.SigningKeys.AddConfiguredKey(new X509Certificate2(Convert.FromBase64String("MIIDAzCCAeugAwIBAgIJLNhPpvvzUXRnMA0GCSqGSIb3DQEBCwUAMB8xHTAbBgNVBAMTFGNjaWRlbnRpdHkuYXV0aDAuY29tMB4XDTIwMDQwMTEwMTEzMVoXDTMzMTIwOTEwMTEzMVowHzEdMBsGA1UEAxMUY2NpZGVudGl0eS5hdXRoMC5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDXOaxVEhjYW+eT3YduAnRMGrJGcriVU1e3n3RXB6SPt4QsmXsOcdLItaqKthSKeRTkOtXvRANvve1YrEUeMwWzv9gsKoQrwM5fDKwG+chEJNxEZEtMmGfqasb++taLTduNPphSm1xs0RNHeeFXdJHt4QWVsCMJfH2RRUlRHaqj4niew/uzJOZNiWLnXp+03tiy5uwOyxyOhpMOX9QZqpSwHHzZ8OoLIIxynuSiWipZoeCoxrZKM3kHW9YlwLVZvcqAZhWTbut6sC+Y+1O1Sfh1tr6XxWzcP5GaMMV2xQPgPYYKjYMiMeFSe4E5P3R4QxmEoeOpAhWzHQbiyzMYLIO1AgMBAAGjQjBAMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFFldi8Vcq5IwPROX5ssxqm+uuQaZMA4GA1UdDwEB/wQEAwIChDANBgkqhkiG9w0BAQsFAAOCAQEA1DWRiUaTCHqPUQRzOOnuiSib2dga2Qd1v39dqaaHrLe345c0eo1yn0cE50xtSTlx++WeIQ3RbXCMm70Za3+AfQhTOisqiHS0Nb+ZrxkHFzl0JNgY4AHQFbYsM3QwZQLBjfhL3KyYoTiQRifn/L9N1F/ZJN5PatoybwwJAbo4V0Y1g9pMSWXhbUKJCBP3Eq/8LPx2erAs3RkheYtz+beviOiXNeNYvUNxmPNy+vpp/zvFG5q20vtK7a3EBbOh1pputoctmAfnGoyjZ06JDAa006iJiGwXlwNonBFClLwbds4H3fc9hv4RsCWlVVdcO+l5FL17nhMRYZUn74lGHOCMZg==")));

                options.IdentityProviders.Add(idp);

                oAuthPostConfigureOptions.PostConfigure(schemeName, options);
                oAuthOptionsCache.TryAdd(schemeName, options);
            }
            else if (schemeData.Scheme == "Saml2")
            {
                IOptionsMonitorCache<Saml2Options> oAuthOptionsCache = serviceProvider.GetRequiredService<IOptionsMonitorCache<Saml2Options>>();
                PostConfigureSaml2Options oAuthPostConfigureOptions = serviceProvider.GetRequiredService<PostConfigureSaml2Options>();

                if (await schemeProvider.GetSchemeAsync(schemeName) == null)
                {
                    schemeProvider.AddScheme(new AuthenticationScheme(schemeName, schemeData.Scheme, typeof(Saml2Handler)));
                }
                else
                {
                    oAuthOptionsCache.TryRemove(schemeName);
                }

                var options = new Saml2Options();
                options.SPOptions.EntityId = new EntityId("https://localhost:44332/Saml2");
                options.SPOptions.MinIncomingSigningAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

                var idp = new IdentityProvider(new EntityId("https://xxxxxxxxxxxx/adfs/services/trust"), options.SPOptions)
                {
                    //MetadataLocation = "https://xxxxxxxxxxxx/FederationMetadata/2007-06/FederationMetadata.xml",
                    //LoadMetadata = true,
                    AllowUnsolicitedAuthnResponse = true,
                    Binding = Saml2BindingType.HttpRedirect,
                    SingleSignOnServiceUrl = new Uri("https://xxxxxxxxx/adfs/ls/"),

                };

                idp.SigningKeys.AddConfiguredKey(new X509Certificate2(Convert.FromBase64String("MIIC5jCCAc6gAwIBAgIQOMQMbu2YTpFIO7bLoDczgjANBgkqhkiG9w0BAQsFADAvMS0wKwYDVQQDEyRBREZTIFNpZ25pbmcgLSBEZXYtMTAxLkJhbmtPZlpvbmUubGswHhcNMjAwMzMwMTQyOTQ5WhcNMjEwMzMwMTQyOTQ5WjAvMS0wKwYDVQQDEyRBREZTIFNpZ25pbmcgLSBEZXYtMTAxLkJhbmtPZlpvbmUubGswggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC0sv1rrY0QcVy8kCYz48dTE0qWlwg7J67kNDuO4um37DKnmSK43QTKMkN4Oe / q6 + a8YV2XW7aHqVzirdyeCWDqWf0fuef0jBhysylwdZI8P8PHAhX632jkQ9dXKqKC9kVEsV + LMzMB98xv3ue + rAjQMctrvdapTgvRTOyu5SEHV7zKN / AXDgqM1AT9ae4prRhg7F37Y6h4DVjCdOZgV7LpmgkkFxFnmk0G5il9yfFnLs2Xw3dQxh8HPj9XCgeNT3GGnui + d69BnESsWDjUBUuBGB / +6WQixC4SnKzbssVTy3W4h3aSSsGljAAJfh5YUafzqCjG7Z6xE16LNBieKjbVAgMBAAEwDQYJKoZIhvcNAQELBQADggEBAH / a2bttVBkWzk4Q7K8qjgC / GQboK1NJewEPdi + 8GKG5RD + hWWz /qmXKT0u6ZklzmNrsxj + jPxIOzlv7Aaa5CbGUHHRoG7mgWnvV7y0Qys3OfRUpIzOK0HzDhe / LlyHyX3TpKDH / b1YQJiE6yHgwEdkO4ZBQsOHDNm9pvWH2YJQqMFbWPA4ZeUASeUO0h + BdR4Fog / MYu86lensZwZUKbq / 1 + M5xao3LZfQh5oyEBpH0roRJOazjMSHV + U4sLdvkvXx6in4BLwt1HiMAm0oA6c + vSW5GANAJBXPupfP6Njt0lpGGC3bLgWOlU65NTPwIZhvAjs / gV / pBa + jVMVxDP0g = ")));

                options.IdentityProviders.Add(idp);

                oAuthPostConfigureOptions.PostConfigure(schemeName, options);
                oAuthOptionsCache.TryAdd(schemeName, options);
            }
            else
            {
                IOptionsMonitorCache<FacebookOptions> oAuthOptionsCache = serviceProvider.GetRequiredService<IOptionsMonitorCache<FacebookOptions>>();
                OAuthPostConfigureOptions<FacebookOptions, FacebookHandler> oAuthPostConfigureOptions = serviceProvider.GetRequiredService<OAuthPostConfigureOptions<FacebookOptions, FacebookHandler>>();

                if (await schemeProvider.GetSchemeAsync(schemeName) == null)
                {
                    schemeProvider.AddScheme(new AuthenticationScheme(schemeName, schemeData.Scheme, typeof(FacebookHandler)));
                }
                else
                {
                    oAuthOptionsCache.TryRemove(schemeName);
                }
                var options = new FacebookOptions
                {
                    AppId = "xxxxxxxxxxxx",
                    AppSecret = "xxxxxxxxxxxxxxxxx"
                };

                oAuthOptionsCache.TryAdd(schemeName, options);
                oAuthPostConfigureOptions.PostConfigure(schemeName, options);
                oAuthOptionsCache.TryAdd(schemeName, options);
            }
            
            return Redirect("/");
        }
    }
}
 