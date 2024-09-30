using FastEndpoints;
using BlazorCodeBase.Server.Database.Model;
using Microsoft.AspNetCore.Identity;

namespace BlazorCodeBase.Server.Handler
{
    public class RegisterCommand : ICommand<IEnumerable<IdentityError>>
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool TwoFactor { get; set; }
        public bool FromGoogle { get; set; }
    }

    public  class RegisterHandler(UserManager<UserInfo> userManager, UserInfoBuilder userInfoBuilder) : ICommandHandler<RegisterCommand, IEnumerable<IdentityError>>
    {
        public async Task<IEnumerable<IdentityError>> ExecuteAsync(RegisterCommand req, CancellationToken ct)
        {
            IdentityResult result = default;
            var user = userInfoBuilder.SetEmail(req.Email)
                                      .SetUserName(req.UserName)
                                      .SetFirstName(req.FirstName)
                                      .SetLastName(req.LastName)
                                      .SetTwoFactorEnabled(true)
                                      .Build();

            if(req.FromGoogle)
                result = await userManager.CreateAsync(user);
            else
                result = await userManager.CreateAsync(user, req.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Guest");
            }
            return result.Errors;
        }
    }
}
