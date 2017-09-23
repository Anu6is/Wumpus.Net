﻿using Wumpus.Serialization;

namespace Wumpus.Entities
{
    public class Presence
    {
        [ModelProperty("user")]
        public User User { get; set; }
        [ModelProperty("guild_id")]
        public Optional<ulong> GuildId { get; set; }
        [ModelProperty("status")]
        public UserStatus Status { get; set; }
        [ModelProperty("game")]
        public Game Game { get; set; }

        [ModelProperty("roles")]
        public Optional<ulong[]> Roles { get; set; }
        [ModelProperty("nick")]
        public Optional<string> Nick { get; set; }
    }
}
