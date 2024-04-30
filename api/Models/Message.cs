using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace api.Models
{
    [FirestoreData]
    public class Message
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [FirestoreProperty]
        public string? Content { get; set; }
        [FirestoreProperty]
        public string? UserId { get; set; }
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public string? ChatSessionId { get; set; }
        [FirestoreProperty]
        public string? role { get; set; }

    }
}