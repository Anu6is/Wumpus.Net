using Wumpus.Serialization;

namespace Wumpus.Entities
{
    public class IntegrationAccount
    {
        [ModelProperty("id")]
        public ulong Id { get; set; }
        [ModelProperty("name")]
        public string Name { get; set; }
    }
}
