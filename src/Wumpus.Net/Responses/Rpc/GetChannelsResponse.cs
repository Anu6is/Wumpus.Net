using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class GetChannelsResponse
    {
        [ModelProperty("channels")]
        public RpcChannelSummary[] Channels { get; set; }
    }
}
