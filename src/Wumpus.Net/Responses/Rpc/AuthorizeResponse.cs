using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class AuthorizeResponse
    {
        [ModelProperty("code")]
        public string Code { get; set; }
    }
}
