using Wumpus.Serialization;

namespace Wumpus.Requests
{
    public class AuthenticateParams
    {
        [ModelProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
