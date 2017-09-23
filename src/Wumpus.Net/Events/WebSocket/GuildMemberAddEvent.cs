using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class GuildMemberAddEvent : GuildMember
    {
        [ModelProperty("guild_id")]
        public ulong GuildId { get; set; }
    }
}
