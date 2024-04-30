using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using api.DTO.Message;
using api.Models;
using api.Repository;

namespace api.Service
{
    public class WebSocketService
    {
        public async static Task HandleWebSocket(HttpContext httpContext)
        {
            ChatRepository chatRepo = (ChatRepository)httpContext.RequestServices.GetRequiredService<IChatRepository>();

            string chatSessionId = httpContext.Request.RouteValues["chatSessionId"] as string;

            if(!await chatRepo.ChatSessionExists(chatSessionId)){
                httpContext.Response.StatusCode = 404;
                return;
            }
            // Handle WebSocket connection for the session
            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

            // Handle WebSocket messages
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Extract message content from WebSocket message
                    string messageContent = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    Message message = new Message
                    { 
                        Content = messageContent,
                        ChatSessionId = chatSessionId, 
                        UserId = "user-tester",
                        role = "User"
                    };

                    // Save message to the database along with the session ID
                    await chatRepo.AddMessageToChatSession(message, chatSessionId);

                    // Example: Echo message back to the client
                    await webSocket.SendAsync(buffer, result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }
        }
    }
}