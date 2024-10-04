using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Endpoint.User;
using BlazorCodeBase.Server.Model.Command;
using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace BlazorCodeBase.Server.Endpoint
{
    class RegisterMailEndpoint(UserManager<UserInfo> userManager, SendMailBuilder mailBuilder) : Endpoint<RegisterMailRequest>
    {
        public override void Configure()
        {
            Post("/Register");
            AllowAnonymous();
            Group<SendMail>();
            Summary(s => {
                s.Summary = "Send register mail";
                s.Description = "This api to send register mail";
                s.Responses[200] = "OK";
            });
        }

        public override async Task HandleAsync(RegisterMailRequest req, CancellationToken ct)
        {
            var user = await userManager.FindByEmailAsync(req.ToEmail);
            if (user is null)
            {
                await SendNotFoundAsync(ct);
            }
            else
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}/api/User/ComfirmRegisterEmail?userName={user.UserName}&email={user.Email}&code={code}";
                var sendMailConfirm = await mailBuilder
                                    .SetTo(user.Email)
                                    .SetSubject("[BlazorCodeBase] Comfirm email register")
                                    .SetUsingTemplate(new FileInfo("Template\\RegisterEmail.cshtml").FullName)
                                    .SetModal(new RegisterTemplateModel(user.UserName, user.FirstName, user.LastName, baseUri))
                                    .Build()
                                    .ExecuteAsync(ct);
                if (!sendMailConfirm.Successful)
                {
                    AddError(string.Join(Environment.NewLine, sendMailConfirm.ErrorMessages));
                }

                ThrowIfAnyErrors();
                await SendOkAsync(ct);
            }
        }
    }

    record RegisterMailRequest(string? ToEmail);

    class RegisterMailRequestValidator : Validator<RegisterMailRequest>
    {
        public RegisterMailRequestValidator()
        {
            RuleFor(x => x.ToEmail)
                .NotEmpty();
        }
    }
}
