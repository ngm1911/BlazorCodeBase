using FastEndpoints;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using BlazorCodeBase.Server.Model.Common;
using Microsoft.Extensions.Options;
using BlazorCodeBase.Server.Model.Command;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BlazorCodeBase.Server.Database.Model;

namespace BlazorCodeBase.Server.Endpoint.Google
{
    class GoogleResponseEndpoint(UserManager<UserInfo> userManager,
                                 IOptionsSnapshot<Settings> settings,
                                 RegisterBuilder registerBuilder,
                                 JwtGenerateBuilder jwtGenerate) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/signin-google");
            Group<GoogleGroup>();
            Summary(s => {
                s.Summary = "GoogleResponse";
                s.Description = "This api to received reponse from google";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                string? name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
                string? surname = result.Principal.FindFirst(ClaimTypes.Surname)?.Value;
                string? givenName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value;
                string? email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                string? userName = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    var errors = await registerBuilder.SetUserName(userName)
                                      .SetEmail(email)
                                      .SetLastName(surname)
                                      .SetFirstName(givenName)
                                      .SetPassword(Guid.NewGuid().ToString())
                                      .SetTwoFactorEnabled(false)
                                      .SetFromGoogle(true)
                                      .Build()
                                      .ExecuteAsync(ct);

                    if (errors.Any())
                    {
                        foreach (var error in errors)
                        {
                            AddError(error.Description);
                        }
                    }
                }
                ThrowIfAnyErrors();
                var jwtToken = await jwtGenerate
                                .SetUserInfo(user!)
                                .SetVerified2FA(true)
                                .Build()
                                .ExecuteAsync(ct);

                HttpContext.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt!.CookieOpt);
                HttpContext.Response.Cookies.Append(Constant.ACCESS_TOKEN, jwtToken, settings.Value.Jwt.CookieOpt);

                await SendRedirectAsync("/");
            }
            else
            {
                await SendUnauthorizedAsync(ct);
            }
        }
    }
}
