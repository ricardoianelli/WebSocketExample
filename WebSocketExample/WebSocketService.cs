using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketExample;

public class WebSocketService
{
    private const int BufferSizeInBytes = 1024;
    private readonly HttpListener _httpListener;

    public event EventHandler<WebSocketMsgReceivedArgs>? MessageReceivedEvent;
    public event EventHandler<WebSocket> WebSocketConnectionStartedEvent;
    public event EventHandler<WebSocket> WebSocketConnectionFinishedEvent;

    private List<WebSocket> _webSocketConnectionsList = new();
    

    public WebSocketService()
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add("http://localhost:5000/");
        
        WebSocketConnectionStartedEvent += OnWebSocketConnectionStartedEvent;
        WebSocketConnectionFinishedEvent += OnWebSocketConnectionFinishedEvent;
    }
    
    private void OnWebSocketConnectionStartedEvent(object? sender, WebSocket webSocket)
    {
        
        _webSocketConnectionsList.Add(webSocket);
    }
    
    private void OnWebSocketConnectionFinishedEvent(object? sender, WebSocket webSocket)
    {
        _webSocketConnectionsList.Remove(webSocket);
    }

    public async Task StartAsync()
    {
        _httpListener.Start();
        Console.WriteLine("Listening for WebSocket connections on http://localhost:5000/");
        
        while (true)
        {
            if (!_httpListener.IsListening) return;
            
            var context = await _httpListener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                await HandleWebSocketClient(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
            }
        }
    }

    public void Stop()
    {
        _httpListener.Stop();
        Console.WriteLine("Stopped listening for WebSocket connections on http://localhost:5000/");
    }

    public async Task SendMessageAsync(WebSocket webSocket, string msg)
    {
        var sendBuffer = Encoding.UTF8.GetBytes(msg);
        await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    private async Task HandleWebSocketClient(HttpListenerContext context)
    {
        var webSocketContext = await context.AcceptWebSocketAsync(null);
        var webSocket = webSocketContext.WebSocket;
        
        WebSocketConnectionStartedEvent?.Invoke(this, webSocket);

        try
        {
            var receiveBuffer = new byte[BufferSizeInBytes];
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    WebSocketConnectionFinishedEvent?.Invoke(this, webSocket);
                    return;
                }
                
               
                var messageBytes = receiveBuffer.Take(receiveResult.Count).ToArray();
                var message = Encoding.UTF8.GetString(messageBytes);

                MessageReceivedEvent?.Invoke(this, new WebSocketMsgReceivedArgs(webSocket, message));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("WebSocket error: " + e.Message);
        }
        finally
        {
            webSocket?.Dispose();
        }
    }


}