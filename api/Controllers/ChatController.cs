using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Account;
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
        [Authorize]
        public async Task<IActionResult> GetChatSessions()
        {
            string jwtToken = HttpContext.Request.Headers["Authorization"].ToString();

            jwtToken = jwtToken.Replace("Bearer ", "");

            string? userId = await _firebaseAuthService.GetUserId(jwtToken);

            if(userId == null) return Unauthorized();

            List<ChatSession> chatSessions = await _chatRepo.GetChatSessions(userId);

            return Ok(chatSessions);
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> CreateChatSession()
        {
            string jwtToken = HttpContext.Request.Headers["Authorization"].ToString();
            Console.WriteLine(jwtToken);

            jwtToken = jwtToken.Replace("Bearer ", "");

            string? userId = await _firebaseAuthService.GetUserId(jwtToken);

            if(userId == null) return Unauthorized();

            CreateChatSessionRequestDTO chatSessionDTO = new CreateChatSessionRequestDTO { UserId = userId };
            ChatSession chatSession = await _chatRepo.CreateChatSession(chatSessionDTO.ToChatSessionFromCreateDTO());

            return Ok(chatSession.ToChatSessionDTO());
        }
    }
}