using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.ChatSession;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Service;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;
        private readonly IFirebaseAuthService _firebaseAuthService;

        public ChatController(IChatRepository chatRepo, IFirebaseAuthService firebaseAuthService)
        {
            _chatRepo = chatRepo;
            _firebaseAuthService = firebaseAuthService;
        }

        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetChatSessions(string userId)
        {
            List<ChatSession> chatSessions = await _chatRepo.GetChatSessions(userId);

            return Ok(chatSessions);
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> CreateChatSession()
        {
            // We will take the user from authorization
            User user = _firebaseAuthService.GetUser();
            CreateChatSessionRequestDTO chatSessionDTO = new CreateChatSessionRequestDTO{UserId = user.Info.Uid};
            ChatSession chatSession = await _chatRepo.CreateChatSession(chatSessionDTO.ToChatSessionFromCreateDTO());

            return Ok(chatSession.ToChatSessionDTO());
        }
    }
}