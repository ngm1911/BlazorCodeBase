using BlazorCodeBase.Server.Handler;

namespace BlazorCodeBase.Server.Model.Command
{
    public class JwtGenerateBuilder
    {
        private readonly JwtGenerateCommand JwtGenerate = new();

        public JwtGenerateBuilder SetUserName(string userName)
        {
            JwtGenerate.UserName = userName;
            return this;
        }
        
        public JwtGenerateBuilder SetEmail(string email)
        {
            JwtGenerate.Email = email;
            return this;
        }

        public JwtGenerateBuilder SetRole(IList<string> role)
        {
            JwtGenerate.Role = role;
            return this;
        }
        
        public JwtGenerateBuilder SetVerified2FA(bool verified2FA)
        {
            JwtGenerate.Verified2FA = verified2FA;
            return this;
        }

        public JwtGenerateCommand Build()
        {
            return JwtGenerate;
        }
    }
}
