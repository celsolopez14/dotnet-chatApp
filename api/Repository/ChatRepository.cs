using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Service;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace api.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly FirestoreDb _chatContext;
        public ChatRepository(FirestoreDb chatContext)
        {
            _chatContext = chatContext;
        }

        public async Task<(Message, Message)> AddMessagesToChatSession(Message userMessage, Message modelMessage, string chatSessionId)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(chatSessionId);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await document.UpdateAsync("Messages", FieldValue.ArrayUnion(userMessage, modelMessage));

                return (userMessage, modelMessage);
            }

            return (null, null);
        }

        public async Task<bool> ChatSessionExists(string Id)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(Id);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            
            return snapshot.Exists ? true : false;
        }

        public async Task<ChatSession> CreateChatSession(ChatSession chatSession)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document();
            chatSession.Id = document.Id;
            await document.SetAsync(chatSession);
            return chatSession;
        }

        public async Task<ChatSession> DeleteChatSession(string Id)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(Id);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                ChatSession chatSession = snapshot.ConvertTo<ChatSession>();
                await document.DeleteAsync();
                return chatSession;
            }

            return null;
        }

        public async Task<ChatSession> GetChatSession(string Id)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(Id);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                ChatSession chatSession = snapshot.ConvertTo<ChatSession>();
                return chatSession;
            }

            return null;
        }

        public async Task<List<ChatSession>> GetChatSessions(string userId)
        {
            Query chatSessionsQuery = _chatContext.Collection("sessions").WhereEqualTo("UserId", userId);
            QuerySnapshot chatSessionsQuerySnapshot = await chatSessionsQuery.GetSnapshotAsync();
            List<ChatSession> chatSessions = new List<ChatSession>();
            foreach (DocumentSnapshot documentSnapshot in chatSessionsQuerySnapshot.Documents)
            {
                ChatSession chatSession = documentSnapshot.ConvertTo<ChatSession>();
                chatSessions.Add(chatSession);
            }
            return chatSessions;
        }

        public async Task<List<Message>> GetMessagesFromChatSession(string Id)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(Id);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists) return snapshot.ConvertTo<ChatSession>().Messages.OrderBy(m => m.CreatedAt).ToList();
            return null;
        }

        public async Task<bool> UserChatSessionExists(string userId, string chatSessionId)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(chatSessionId);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                ChatSession chatSession = snapshot.ConvertTo<ChatSession>();
                return chatSession.UserId == userId;
            }
            return false;
        }
    }
}