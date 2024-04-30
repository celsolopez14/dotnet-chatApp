using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.ChatSession;
using api.Models;

namespace api.Mappers
{
    public static class ChatSessionMappers
    {
        public static ChatSession ToChatSessionFromCreateDTO(this CreateChatSessionRequestDTO chatSessionDTO){
            return new ChatSession{
                UserId = chatSessionDTO.UserId,
                CreatedAt = chatSessionDTO.CreatedAt,
            };
        }

        public static ChatSessionDTO ToChatSessionDTO(this ChatSession chatSession){
            return new ChatSessionDTO{
                Id = chatSession.Id
            };
        }
        
    }
}