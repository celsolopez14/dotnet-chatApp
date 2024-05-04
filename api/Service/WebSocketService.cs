using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using api.DTO.Message;
using api.Interfaces;
using api.Models;
using api.Repository;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace api.Service
{
    public class WebSocketService
    {
        public async static Task HandleWebSocket(HttpContext httpContext)
        {
            IChatRepository chatRepo = httpContext.RequestServices.GetRequiredService<IChatRepository>();
            IGeminiAIService geminiAIService = httpContext.RequestServices.GetRequiredService<IGeminiAIService>();
            IFirebaseAuthService firebaseAuthService = httpContext.RequestServices.GetRequiredService<IFirebaseAuthService>();

            AuthenticateResult authenticateResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || !firebaseAuthService.IsUserSignedIn())
            {
                httpContext.Response.StatusCode = 401;
                return;
            }
            string chatSessionId = httpContext.Request.RouteValues["chatSessionId"] as string;

            if (!await chatRepo.ChatSessionExists(chatSessionId))
            {
                httpContext.Response.StatusCode = 404;
                return;
            }

            var userId = firebaseAuthService.GetUser().Uid;

            if (!await chatRepo.UserChatSessionExists(userId, chatSessionId))
            {
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
                    Message userMessage = new Message
                    {
                        Content = messageContent,
                        ChatSessionId = chatSessionId,
                        role = "user"
                    };
                    Content response = new Content();
                    if (geminiAIService.IsContentEmpty())
                    {
                        List<Message> messages = await chatRepo.GetMessagesFromChatSession(chatSessionId);
                        messages.Add(userMessage);
                        response = await geminiAIService.GenerateFirstContent(messages);
                    } else{
                        response = await geminiAIService.GenerateContent(userMessage);
                    }

                    var bufferResponse = Encoding.UTF8.GetBytes(response.Parts.Last().Text);

                    Message modelMessage = new Message
                    {
                        Content = response.Parts.Last().Text,
                        ChatSessionId = chatSessionId,
                        role = "model"
                    };
                    await chatRepo.AddMessagesToChatSession(userMessage, modelMessage, chatSessionId);
                    // Example: Echo message back to the client
                    await webSocket.SendAsync(bufferResponse, result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }
        }
    }
}