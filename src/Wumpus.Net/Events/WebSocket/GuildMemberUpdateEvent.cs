using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class GuildMemberUpdateEvent : GuildMember
    {
        [ModelProperty("guild_id")]
        public ulong GuildId { get; set; }
    }
}
