﻿using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class RecipientEvent
    {
        [ModelProperty("user")]
        public User User { get; set; }
        [ModelProperty("channel_id")]
        public ulong ChannelId { get; set; }
    }
}
