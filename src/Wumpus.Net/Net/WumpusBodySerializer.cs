using RestEase;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Wumpus.Requests;
using Wumpus.Serialization.Json;

namespace Wumpus.Net
{
    internal class WumpusBodySerializer : RequestBodySerializer
    {
        private readonly DiscordJsonSerializer _serializer = new DiscordJsonSerializer();

        public override HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info)
        {
            if (body == null)
                return null;

            if (body is IFormData form)
            {
                var pairs = form.GetFormData();
                var content = new MultipartFormDataContent();
                foreach (var pair in pairs)
                {
                    if (pair.Value is MultipartFile file)
                    {
                        var stream = file.Stream;
                        if (!stream.CanSeek)
                        {
                            var memoryStream = new MemoryStream();
                            file.Stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            stream = memoryStream;
                        }
                        content.Add(new StreamContent(stream), pair.Key, file.Filename);
                    }
                    else
                        content.Add(new StringContent(_serializer.WriteToString(pair.Value, pair.Value.GetType()), Encoding.UTF8), pair.Key);
                }
                return content;
            }
            else
            {
                var arr = _serializer.WriteToArray(body, 1024, SymbolTable.InvariantUtf8);
                var content = new ByteArrayContent(arr.Array, arr.Offset, arr.Count);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return content;
            }
        }
    }
}
