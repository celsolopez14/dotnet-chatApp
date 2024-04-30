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
        Task<ChatSession> GetChatSession(string Id);
        Task<ChatSession> CreateChatSession(ChatSession chatSession);
        Task<Message> AddMessageToChatSession(Message messageModel, string chatSessionId);
        Task<ChatSession> DeleteChatSession(string Id);

        Task<bool> ChatSessionExists(string Id);

    }
}