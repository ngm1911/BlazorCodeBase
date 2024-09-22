using BlazorCodeBase.Server.Model.Common;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Net;
using BlazorCodeBase.Server.Database.Model;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class VerifyTwoFactorCodeEndpoint(UserManager<UserInfo> userManager,
                        JwtGenerateBuilder jwtGenerate, 
                        IOptionsSnapshot<Settings> settings) : Endpoint<VerifyTwoFactorCodeRequest>
    {
        public override void Configure()
        {
            Post("/VerifyTwoFactorCode");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "VerifyTwoFactor";
                s.Description = "This api to Verify 2FA";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(VerifyTwoFactorCodeRequest req, CancellationToken ct)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if (user is null)
            {
                await SendOkAsync(Responses.UserNotFound, ct);
            }
            else
            {
                if (await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, req.Code) == false)
                {
                    await SendOkAsync(Responses.UnAuthorized, ct);
                }
                else
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var jwtToken = await jwtGenerate
                                            .SetUserName(user.UserName)
                                            .SetEmail(user.Email)
                                            .SetRole(roles)
                                            .SetVerified2FA(true)
                                            .Build()
                                            .ExecuteAsync(ct);

                    HttpContext.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt!.CookieOpt);
                    HttpContext.Response.Cookies.Append(Constant.ACCESS_TOKEN, jwtToken, settings.Value.Jwt.CookieOpt);

                    await SendOkAsync(Responses.OK, ct);
                }
            }
        }
    }

    class VerifyTwoFactorCodeRequest
    {
        public string Code { get; set; }
    }

    class VerifyTwoFactorCodeValidator : Validator<VerifyTwoFactorCodeRequest>
    {
        public VerifyTwoFactorCodeValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty();
        }
    }
}
