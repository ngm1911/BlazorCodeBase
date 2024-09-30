using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Handler;

namespace BlazorCodeBase.Server.Model.Command
{
    public class JwtGenerateBuilder
    {
        private readonly JwtGenerateCommand JwtGenerate = new();

        public JwtGenerateBuilder SetUserInfo(UserInfo userInfo)
        {
            JwtGenerate.UserInfo = userInfo;
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
