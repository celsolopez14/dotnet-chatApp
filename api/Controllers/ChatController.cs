using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.ChatSession;
using api.Mappers;
using api.Models;
using api.Service;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;

        public ChatController(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetChatSessions(string userId)
        {
            List<ChatSession> chatSessions = await _chatRepo.GetChatSessions(userId);

            return Ok(chatSessions);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateChatSession()
        {
            // We will take the user from authorization
            CreateChatSessionRequestDTO chatSessionDTO = new CreateChatSessionRequestDTO{UserId = "user-tester"};
            ChatSession chatSession = await _chatRepo.CreateChatSession(chatSessionDTO.ToChatSessionFromCreateDTO());

            return Ok(chatSession.ToChatSessionDTO());
        }
    }
}