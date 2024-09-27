using FastEndpoints;

namespace BlazorCodeBase.Server.Endpoint.Google
{
    public class GoogleGroup : Group
    {
        public GoogleGroup()
        {
            Configure("Google", ep =>             
            {
                ep.Description(x => x
                  .WithTags("Google"));
            });
        }
    }
}
