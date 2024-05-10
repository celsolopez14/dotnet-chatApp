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

            string jwtToken = httpContext.Request.Query["Authorization"].ToString();

            if (!await firebaseAuthService.IsTokenValid(jwtToken))
            {
                httpContext.Response.StatusCode = 401;
                return;
            }

            string? userId = await firebaseAuthService.GetUserId(jwtToken);
            string? chatSessionId = httpContext.Request.RouteValues["chatSessionId"].ToString();

            if (chatSessionId == null || userId == null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }

            if (!await chatRepo.ChatSessionExists(chatSessionId) && !await chatRepo.UserChatSessionExists(userId, chatSessionId))
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
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Extract message content from WebSocket message
                    string messageContent = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    Message userMessage = new Message
                    {
                        Content = messageContent,
                        ChatSessionId = chatSessionId,
                        Role = "user"
                    };
                    Content response = new Content();
                    // If the app just started
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
                        Role = "model"
                    };
                    await chatRepo.AddMessagesToChatSession(userMessage, modelMessage, chatSessionId);
                    // Returns gemini response
                    await webSocket.SendAsync(bufferResponse, result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }
        }
    }
}