using Wumpus.Serialization;

namespace Wumpus.Requests
{
    public class SetLocalVolumeParams
    {
        [ModelProperty("volume")]
        public int Volume { get; set; }
    }
}
