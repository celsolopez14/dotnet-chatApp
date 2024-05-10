using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace api.Service
{
    public interface IChatRepository
    {
        Task<List<ChatSession>> GetChatSessions(string userId);
        Task<ChatSession> CreateChatSession(ChatSession chatSession);
        Task<(Message, Message)> AddMessagesToChatSession(Message userMessage, Message modelMessage, string chatSessionId);
        Task<List<Message>> GetMessagesFromChatSession(string Id);

        Task<ChatSession> DeleteChatSession(string Id);

        Task<bool> ChatSessionExists(string Id);
        Task<bool> UserChatSessionExists(string userId, string chatSessionId);

    }
}