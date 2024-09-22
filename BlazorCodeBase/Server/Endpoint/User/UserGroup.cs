using FastEndpoints;

namespace BlazorCodeBase.Server.Endpoint.User
{
    public class UserGroup : Group
    {
        public UserGroup()
        {
            Configure("User", ep =>             
            {
                ep.Description(x => x
                  .WithTags("User"));
            });
        }
    }
}
