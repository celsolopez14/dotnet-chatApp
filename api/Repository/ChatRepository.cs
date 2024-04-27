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

        public async Task<Message> AddMessageToChatSession(Message messageModel, string chatSessionId)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(chatSessionId);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if(snapshot.Exists){
                ChatSession chatSession = snapshot.ConvertTo<ChatSession>();
                List<Message> messages = chatSession.Messages;
                messages.Add(messageModel);

                await document.UpdateAsync("Messages", messages);

                return messageModel;
            }

            return null;
        }

        public async Task<Message> CreateChatSession(Message messageModel)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document();
            messageModel.ChatSessionId = document.Id;
            ChatSession newChatSession = new ChatSession{
                Messages = new List<Message>{messageModel},
            };


            await document.SetAsync(newChatSession);
            return messageModel;
        }

        public async Task<ChatSession> DeleteChatSession(string Id)
        {
            DocumentReference document = _chatContext.Collection("sessions").Document(Id);
            DocumentSnapshot snapshot = await document.GetSnapshotAsync();
            if(snapshot.Exists){
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
            if(snapshot.Exists){
                ChatSession chatSession = snapshot.ConvertTo<ChatSession>();
                return chatSession;
            }

            return null;
        }

        public async Task<List<ChatSession>> GetChatSessions()
        {
            Query collection = _chatContext.Collection("sessions");
            QuerySnapshot allChatSessionsSnapshot = await collection.GetSnapshotAsync();
            List<ChatSession> chatSessions = new List<ChatSession>();
            foreach(DocumentSnapshot documentSnapshot in allChatSessionsSnapshot.Documents){
                ChatSession chatSession = documentSnapshot.ConvertTo<ChatSession>();
                chatSessions.Add(chatSession);
            }

            return chatSessions;
        }
    }
}