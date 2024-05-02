using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Google.Cloud.AIPlatform.V1;

namespace api.Interfaces
{
    public interface IGeminiAIService
    {
        Task<List<Content>> GenerateContent(List<Message> messages);
    }
}