using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Message;
using api.Models;
using Google.Cloud.AIPlatform.V1;

namespace api.Mappers
{
    public static class MessageMappers
    {
        public static Message ToMessageFromSendDTO(this SendMessageRequestDTO messageDTO){
            return new Message{
                Content = messageDTO.Content,
            };
        }

        public static MessageDTO ToMessageDTOFromMessage(this Message message){
            return new MessageDTO{
                Content = message.Content,
                ChatSessionId = message.ChatSessionId,
                CreatedAt = message.CreatedAt
            };
        }

        public static Content ToContentFromMessage(this Message message){
            Content content = new Content{
                Role = message.role,
            };
            content.Parts.Add(new Part{Text = message.Content});
            return content;
        }
    }
}