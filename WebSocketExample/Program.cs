using System.Net.WebSockets;

namespace WebSocketExample
{
    class Program
    {
        private static WebSocketService? _webSocketService;
        
        static async Task Main(string[] args)
        {
            _webSocketService = new WebSocketService();
            
            _webSocketService.MessageReceivedEvent += OnWebSocketMessageReceivedEvent;
            _webSocketService.WebSocketConnectionStartedEvent += OnWebSocketConnectionStartedEvent;
            _webSocketService.WebSocketConnectionFinishedEvent += OnWebSocketConnectionFinishedEvent;
            
            await _webSocketService.StartAsync();
        }

        private static void OnWebSocketConnectionStartedEvent(object? sender, WebSocket webSocket)
        {
            Console.WriteLine("Started a new WebSocket connection.");
        }
        
        private static void OnWebSocketConnectionFinishedEvent(object? sender, WebSocket webSocket)
        {
            Console.WriteLine("Ended a WebSocket connection.");
        }

        private static void OnWebSocketMessageReceivedEvent(object? sender, WebSocketMsgReceivedArgs msgArgs)
        {
            Console.WriteLine("Message received: " + msgArgs.Message);

            var echoedMsg = "Echo: " + msgArgs.Message;
            _webSocketService?.SendMessageAsync(msgArgs.WebSocket, echoedMsg);
        }

    }
}
