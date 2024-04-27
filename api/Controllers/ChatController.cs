using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Message;
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
        public async Task<IActionResult> GetChatSessions()
        {
            List<ChatSession> chatSessions = await _chatRepo.GetChatSessions();

            return Ok(chatSessions);
        }

        [HttpPost()]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDTO messageDTO, [FromQuery] string? chatSessionId = null)
        {
            Message messageModel = messageDTO.ToMessageFromSendDTO();

            if (chatSessionId == null)
            {
                messageModel.UserId = "user-tester";
                messageModel = await _chatRepo.CreateChatSession(messageModel);
            }
            else
            {
                ChatSession chatSession = await _chatRepo.GetChatSession(chatSessionId);

                if (chatSession == null) return BadRequest($"Could not find a chat sessio with this id: ${chatSessionId}");

                messageModel.ChatSessionId = chatSessionId;

                messageModel.UserId = "user-tester";

                await _chatRepo.AddMessageToChatSession(messageModel, chatSessionId);
            }

            return Ok(messageModel.ToMessageDTOFromMessage());
        }
    }
}