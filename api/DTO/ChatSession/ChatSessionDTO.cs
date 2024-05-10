using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Message;

namespace api.DTO.ChatSession
{
    public class ChatSessionDTO
    {
        public string? Id { get; set; }
        public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();

    }
}