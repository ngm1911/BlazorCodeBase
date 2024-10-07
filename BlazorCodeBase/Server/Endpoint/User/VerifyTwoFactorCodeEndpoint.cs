using BlazorCodeBase.Server.Model.Common;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Net;
using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Shared;

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
            if (string.IsNullOrWhiteSpace(User?.Identity?.Name))
            {
                await SendNotFoundAsync(ct);
            }
            else
            {
                var user = await userManager.FindByNameAsync(User?.Identity?.Name);
                if (user is null)
                {
                    await SendNotFoundAsync(ct);
                }
                else
                {
                    if (await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, req.Code) == false)
                    {
                        await SendUnauthorizedAsync(ct);
                    }
                    else
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        var jwtToken = await jwtGenerate
                                                .SetUserInfo(user)
                                                .SetVerified2FA(true)
                                                .Build()
                                                .ExecuteAsync(ct);

                        HttpContext.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt!.CookieOpt);
                        HttpContext.Response.Cookies.Append(Constant.ACCESS_TOKEN, jwtToken, settings.Value.Jwt.CookieOpt);

                        var userInfoResponse = new UserInfoResponse(user.FirstName,
                                                           user.FirstName,
                                                           user.Email,
                                                           user.UserName,
                                                           roles);
                        await SendCreatedAtAsync(nameof(GetUserInfoEndpoint), new { user.Email }, userInfoResponse, cancellation: ct);
                    }
                }
            }
        }
    }

    record VerifyTwoFactorCodeRequest(string? Code);

    class VerifyTwoFactorCodeValidator : Validator<VerifyTwoFactorCodeRequest>
    {
        public VerifyTwoFactorCodeValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty();
        }
    }
}
