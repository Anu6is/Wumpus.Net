using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class GuildBanEvent
    {
        [ModelProperty("guild_id")]
        public ulong GuildId { get; set; }
        [ModelProperty("user")]
        public User User { get; set; }
    }
}
