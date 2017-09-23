using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class GetGuildsResponse
    {
        [ModelProperty("guilds")]
        public RpcGuildSummary[] Guilds { get; set; }
    }
}
