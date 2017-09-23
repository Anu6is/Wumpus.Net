using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Requests
{
    public class ModifyMessageParams
    {
        [ModelProperty("content")]
        public Optional<string> Content { get; set; }
        [ModelProperty("embed")]
        public Optional<Embed> Embed { get; set; }

        public void Validate()
        {
            if (!Content.IsSpecified || Content.Value == null)
                Content = "";
            if (Embed.IsSpecified && Embed.Value != null)
                Preconditions.NotNullOrWhitespace(Content, nameof(Content));
            // else //TODO: Validate embed length
            Preconditions.LengthAtMost(Content, DiscordConstants.MaxMessageSize, nameof(Content));
        }
    }
}
