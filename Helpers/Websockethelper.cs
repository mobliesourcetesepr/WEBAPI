using System.Net.WebSockets;
using System.Text;

namespace WebSocketDemo
{
    public static class WebSocketHelper
    {
        public static async Task EchoMessages(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                string reply = $"Echo: {message}";
                byte[] responseBytes = Encoding.UTF8.GetBytes(reply);

                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
