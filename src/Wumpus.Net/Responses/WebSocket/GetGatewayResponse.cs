using Wumpus.Serialization;

namespace Wumpus.Responses
{
    public class GetGatewayResponse
    {
        [ModelProperty("url")]
        public string Url { get; set; }
    }
}
