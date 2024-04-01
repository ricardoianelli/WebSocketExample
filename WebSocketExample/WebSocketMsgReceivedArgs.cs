using System.Net.WebSockets;

namespace WebSocketExample;

public record WebSocketMsgReceivedArgs(WebSocket WebSocket, string Message);