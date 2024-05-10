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
        public static MessageDTO ToMessageDTOFromMessage(this Message message){
            return new MessageDTO{
                Content = message.Content,
                Id = message.Id,
                CreatedAt = message.CreatedAt,
                Role = message.Role
            };
        }

        public static Content ToContentFromMessage(this Message message){
            Content content = new Content{
                Role = message.Role,
            };
            content.Parts.Add(new Part{Text = message.Content});
            return content;
        }
    }
}