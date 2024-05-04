using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;

namespace api.Service
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly PredictionServiceClient _predictionServiceClient;
        private readonly GenerateContentRequest _generateContentRequest;
        public GeminiAIService(PredictionServiceClient predictionServiceClient, GenerateContentRequest generateContentRequest)
        {
            _predictionServiceClient = predictionServiceClient;
            _generateContentRequest = generateContentRequest;
        }

        public async Task<Content> GenerateFirstContent(List<Message> messages)
        {
            List<Content> contents = new List<Content>();
            Console.WriteLine("Messages: ");
            foreach (Message message in messages)
            {
                contents.Add(message.ToContentFromMessage());
                Console.WriteLine(message.ToContentFromMessage());
            }

            _generateContentRequest.Contents.AddRange(contents);
            Console.WriteLine("Content: ");
            foreach (Content content in _generateContentRequest.Contents)
            {
                Console.WriteLine(content);
            }

            // Make the request, returning a streaming response
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(_generateContentRequest);
            _generateContentRequest.Contents.Add(response.Candidates[0].Content);

            return response.Candidates[0].Content;
        }

        public async Task<Content> GenerateContent(Message messages)
        {
            _generateContentRequest.Contents.Add(messages.ToContentFromMessage());
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(_generateContentRequest);
            _generateContentRequest.Contents.Add(response.Candidates[0].Content);

            return response.Candidates[0].Content;
        }

        public bool IsContentEmpty() {
            return _generateContentRequest.Contents.Count == 0;
        }

    }
}