using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace api.DTO.Message
{
    public class MessageDTO
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}