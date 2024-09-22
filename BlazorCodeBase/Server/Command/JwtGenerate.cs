using FastEndpoints;
using FastEndpoints.Security;
using BlazorCodeBase.Server.Model.Common;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BlazorCodeBase.Server.Handler
{
    public class JwtGenerateCommand : ICommand<string>
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool Verified2FA { get; set; }
        public IList<string> Role { get; set; }
    }

    public class JwtGenerateHandler(IOptionsSnapshot<Settings> settings) : ICommandHandler<JwtGenerateCommand, string>
    {
        public async Task<string> ExecuteAsync(JwtGenerateCommand command, CancellationToken ct)
        {
            var jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    o.SigningKey = settings.Value.Jwt.Key;
                    o.ExpireAt = settings.Value.Jwt.ExpireTime.HasValue ? DateTime.UtcNow.AddMinutes(settings.Value.Jwt.ExpireTime.Value) : null;
                    o.User.Claims.Add((ClaimTypes.Name, command.UserName));
                    o.User.Claims.Add((ClaimTypes.Email, command.Email));
                    foreach (var item in command.Role)
                    {
                        o.User.Claims.Add((ClaimTypes.Role, item));
                    }
                    if (command.Verified2FA)
                    {
                        o.User.Claims.Add(("amr", "mfa"));
                    }
                });
            return await Task.FromResult(jwtToken);
        }
    }
}
