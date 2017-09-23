using Wumpus.Serialization;

namespace Wumpus.Events
{
    public class SpeakingEvent
    {
        [ModelProperty("user_id")]
        public ulong UserId { get; set; }
    }
}
