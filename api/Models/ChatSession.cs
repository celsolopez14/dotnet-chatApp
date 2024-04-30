using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;


namespace api.Models
{
    [FirestoreData]
    public class ChatSession
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }
        [FirestoreProperty]
        public string? UserId { get; set; }
        [FirestoreProperty]
        public List<Message> Messages { get; set; } = new List<Message>();
        [FirestoreDocumentCreateTimestamp]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}