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
        
        public async Task<List<Content>> GenerateContent(List<Message> messages)
        {
            List<Content> contents = new List<Content>();

            foreach (Message message in messages)
            {
                contents.Add(message.ToContentFromMessage());
            }

            _generateContentRequest.Contents.AddRange(contents);

            // Make the request, returning a streaming response
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(_generateContentRequest);
            contents.Add(response.Candidates[0].Content);

            return contents;
        }

    }
}