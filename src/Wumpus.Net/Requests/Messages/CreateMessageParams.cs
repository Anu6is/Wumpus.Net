﻿using System.Collections.Generic;
using Wumpus.Entities;
using Wumpus.Serialization;

namespace Wumpus.Requests
{
    public class CreateMessageParams : IFormData
    {
        [ModelProperty("content")]
        public Optional<string> Content { get; set; }
        [ModelProperty("nonce")]
        public Optional<string> Nonce { get; set; }
        [ModelProperty("tts")]
        public Optional<bool> IsTTS { get; set; }
        [ModelProperty("embed")]
        public Optional<Embed> Embed { get; set; }

        public Optional<MultipartFile> File { get; set; }
        
        public IDictionary<string, object> GetFormData()
        {
            var dict = new Dictionary<string, object>();
            if (File.IsSpecified)
                dict["file"] = File.Value;
            dict["payload_json"] = this;
            return dict;
        }

        public void Validate()
        {
            Preconditions.NotNull(Nonce, nameof(Nonce));

            if (!Content.IsSpecified || Content.Value == null)
                Content = "";
            if (Embed.IsSpecified && Embed.Value != null)
                Preconditions.NotNullOrWhitespace(Content, nameof(Content));
            // else //TODO: Validate embed length
            Preconditions.LengthAtMost(Content, DiscordConstants.MaxMessageSize, nameof(Content));
        }
    }
}
