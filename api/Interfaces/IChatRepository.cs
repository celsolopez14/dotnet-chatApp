using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Service
{
    public interface IChatRepository
    {
        Task<List<ChatSession>> GetChatSessions();
        Task<ChatSession> GetChatSession(string Id);
        Task<Message> CreateChatSession(Message messageModel);
        Task<Message> AddMessageToChatSession(Message messageModel, string chatSessionId);
        Task<ChatSession> DeleteChatSession(string Id);

    }
}