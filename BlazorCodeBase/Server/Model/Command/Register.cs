using BlazorCodeBase.Server.Database.Model;
using BlazorCodeBase.Server.Endpoint.User;
using BlazorCodeBase.Server.Handler;

namespace BlazorCodeBase.Server.Model.Command
{
    public class RegisterBuilder
    {
        private readonly RegisterCommand registerCommand = new();

        public RegisterBuilder SetUserName(string userName)
        {
            registerCommand.UserName = userName;
            return this;
        }
        
        public RegisterBuilder SetEmail(string email)
        {
            registerCommand.Email = email;
            return this;
        }

        public RegisterBuilder SetFirstName(string firstName)
        {
            registerCommand.FirstName = firstName;
            return this;
        }

        public RegisterBuilder SetLastName(string lastName)
        {
            registerCommand.LastName = lastName;
            return this;
        }

        public RegisterBuilder SetPassword(string password)
        {
            registerCommand.Password = password;
            return this;
        }

        public RegisterBuilder SetFromGoogle(bool fromGoogle)
        {
            registerCommand.FromGoogle = fromGoogle;
            return this;
        }
        
        public RegisterBuilder SetTwoFactorEnabled(bool enable)
        {
            registerCommand.TwoFactor = enable;
            return this;
        }

        public RegisterCommand Build()
        {
            return registerCommand;
        }
    }
}
