using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace WebChatBot.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;

        public SemanticKernelService(IConfiguration configuration)
        {
            string apiKey = configuration["SemanticKernel:ApiKey"];
            string endPoint = configuration["SemanticKernel:Endpoint"];
            string deploymentName = configuration["SemanticKernel:DeploymentName"];

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endPoint,
                apiKey: apiKey
            );

            _kernel = builder.Build();
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            var result = await _kernel.InvokePromptAsync(userMessage);
            Console.WriteLine(result);
            return result.ToString();
        }
    }
}