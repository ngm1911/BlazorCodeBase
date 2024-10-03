using FastEndpoints;
using FastEndpoints.Security;
using BlazorCodeBase.Server.Model.Common;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Client;
using Microsoft.AspNetCore.Identity;

namespace BlazorCodeBase.Server.Handler
{
    public class JwtGenerateCommand : ICommand<string>
    {
        public UserInfo? UserInfo { get; set; }

        public bool Verified2FA { get; set; }
    }

    public class JwtGenerateHandler(UserManager<UserInfo> userManager, IOptionsSnapshot<Settings> settings) : ICommandHandler<JwtGenerateCommand, string>
    {
        public async Task<string> ExecuteAsync(JwtGenerateCommand command, CancellationToken ct)
        {
            var roles = await userManager.GetRolesAsync(command.UserInfo);
            var jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    o.SigningKey = settings.Value.Jwt.Key;
                    o.ExpireAt = settings.Value.Jwt.ExpireTime.HasValue ? DateTime.UtcNow.AddMinutes(settings.Value.Jwt.ExpireTime.Value) : null;
                    o.User.Claims.Add((ClaimTypes.Name, command.UserInfo.UserName));
                    o.User.Claims.Add((ClaimTypes.Email, command.UserInfo.Email));
                    foreach (var item in roles)
                    {
                        o.User.Claims.Add((ClaimTypes.Role, item));
                    }
                    if (command.Verified2FA)
                    {
                        o.User.Claims.Add((ClaimTypes.AuthenticationMethod, "mfa"));
                    }
                });

            return await Task.FromResult(jwtToken);
        }
    }
}
