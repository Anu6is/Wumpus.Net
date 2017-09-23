using System.Net.Http;
using RestEase;
using Wumpus.Serialization.Json;
using System.Text.Utf8;

namespace Wumpus.Net
{
    internal class WumpusResponseDeserializer : ResponseDeserializer
    {
        private readonly DiscordJsonSerializer _serializer = new DiscordJsonSerializer();

        public override T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)
            => _serializer.Read<T>(new Utf8String(content));
    }
}
