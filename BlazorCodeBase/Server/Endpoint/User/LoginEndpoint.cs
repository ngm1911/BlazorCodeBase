using BlazorCodeBase.Server.Model.Common;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Net;
using BlazorCodeBase.Server.Database.Model;
using Serilog;

namespace BlazorCodeBase.Server.Endpoint.User
{
    class LoginEndpoint(UserManager<UserInfo> userManager,
                        SignInManager<UserInfo> signInManager,
                        JwtGenerateBuilder jwtGenerate,
                        SendMailBuilder mailBuilder,
                        IOptionsSnapshot<Settings> settings) : Endpoint<LoginRequest>
    {
        public override void Configure()
        {
            Post("/Login");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Login";
                s.Description = "This api to login";
                s.Responses[(int)HttpStatusCode.OK] = "OK";
            });
            Group<UserGroup>();
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            UserInfo? user = await userManager.FindByNameAsync(req.UserName);
            if (user is null)
            {
                await SendOkAsync(Responses.UserNotFound, ct);
            }
            else
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
                if (result.IsLockedOut)
                {
                    await SendOkAsync(Responses.LockedOut, ct);
                }
                else if(result.IsNotAllowed)
                {
                    await SendOkAsync(Responses.NeedConfirmEmail, ct);
                }
                else if(!result.Succeeded)
                {
                    await SendOkAsync(Responses.UnAuthorized, ct);
                }
                else
                {
                    var jwtToken = await jwtGenerate
                                            .SetUserInfo(user)
                                            .SetVerified2FA(false)
                                            .Build()
                                            .ExecuteAsync(ct);

                    HttpContext.Response.Cookies.Delete(Constant.ACCESS_TOKEN, settings.Value.Jwt!.CookieOpt);
                    HttpContext.Response.Cookies.Append(Constant.ACCESS_TOKEN, jwtToken, settings.Value.Jwt.CookieOpt);

                    var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                    var sendMailConfirm = await mailBuilder
                            .SetTo(user.Email)
                            .SetSubject("[BlazorCodeBase] Verify 2FA")
                            .SetBody($"Your 2FA code: {code}")
                            .Build()
                            .ExecuteAsync(ct);
                    if (!sendMailConfirm.Successful)
                    {
                        AddError(string.Join(Environment.NewLine, sendMailConfirm.ErrorMessages));
                    }
                    await SendOkAsync(Responses.RequiresTwoFactor, ct);
                }
            }
        }
    }

    record LoginRequest (string? UserName, string? Password);

    class LoginValidator : Validator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.UserName)
                .NotEmpty();
        }
    }
}
