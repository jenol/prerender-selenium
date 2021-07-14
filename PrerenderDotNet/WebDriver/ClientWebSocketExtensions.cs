using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrerenderDotNet.WebDriver
{
    public static class ClientWebSocketExtensions
    {
        public static async Task SendAsJson<T>(this ClientWebSocket client, T obj, CancellationToken cancellationToken)
        {
            var jsonText = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(jsonText);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        public static async Task<string> ReceiveTextAsync(this ClientWebSocket client, CancellationToken cancellationToken)
        {
            var rbuffer = new ArraySegment<byte>(new Byte[8192]);
            using var ms = new MemoryStream();
            WebSocketReceiveResult result = null;
            do
            {
                result = await client.ReceiveAsync(rbuffer, cancellationToken);
                ms.Write(rbuffer.Array, rbuffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);
            ms.Seek(0, SeekOrigin.Begin);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                case WebSocketMessageType.Close:
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                    return null;
                default: throw new Exception($"Unhandled MessageType: {result.MessageType}!");
            }
        }

        public static async Task<T> ReceiveJsonAsync<T>(this ClientWebSocket client, CancellationToken cancellationToken)
        {
            var text = await client.ReceiveTextAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(text) ? default : JsonConvert.DeserializeObject<T>(text);
        }
    }
}
