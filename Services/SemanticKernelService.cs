using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.VisualBasic;
using WebChatBot.Models;
using WebChatBot.Plugins;

namespace WebChatBot.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;

        private readonly KernelPlugin _appointmentPlugin;

        private const string SYSTEM_PROMPT = @"You are an intelligent appointment scheduling assistant for a medical clinic. 
        Your primary goal is to help patients schedule appointments with appropriate doctors.

        Follow these guidelines:
        1. If this is a new conversation, greet the user and ask how you can help with scheduling.
        2. Collect the following information (if not already provided):
        - Patient's name
        - Doctor specialty needed (e.g., cardiologist, dermatologist, general practitioner)
        - Preferred date and time for the appointment
        - Reason for the visit (brief description of symptoms or concerns)
        - Patient's contact information (phone or email)

        3. Once you have all necessary information, summarize the appointment details and confirm with the patient.
        4. Be conversational and empathetic, understanding that medical issues can be sensitive.
        5. If the user asks questions about medical conditions, politely explain that you're an appointment scheduling assistant and cannot provide medical advice.
        6. If the user wants to reschedule or cancel, ask for the appointment details and confirm the change.

        Available specialties at our clinic:
        - General Practice
        - Cardiology
        - Dermatology
        - Orthopedics
        - Pediatrics
        - Neurology
        - Gynecology
        - Ophthalmology
        - ENT (Ear, Nose, Throat)

        Available appointment hours: Monday-Friday, 9:00 AM to 5:00 PM

        You can use tools to help schedule appointments:
        - Extract appointment details from the conversation
        - Validate if all required appointment information is present
        - Check availability for a specialty
        - Book appointments when all details are complete";

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

            // Register the appointment plugin
            var appointmentPlugin = new AppointmentPlugin(_kernel);
            _appointmentPlugin = KernelPluginFactory.CreateFromObject(appointmentPlugin, "AppointmentPlugin");
            _kernel.Plugins.Add(_appointmentPlugin);
        }

        public async Task<string> GetChatResponseAsync(string userMessage, IEnumerable<ChatMessage> messages)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

             ChatHistory chatHistory = new ChatHistory();
            
            // Add the system prompt
            chatHistory.AddSystemMessage(SYSTEM_PROMPT);

            foreach(var msg in messages) {
                if (msg.Sender == "user") {
                    chatHistory.AddUserMessage(msg.Message);
                } else {
                    chatHistory.AddAssistantMessage(msg.Message);
                }
            }

            //chatHistory.AddUserMessage(userMessage);

             // Create the full conversation text for extraction
            StringBuilder conversationText = new StringBuilder();
            foreach (var msg in chatHistory)
            {
                conversationText.AppendLine($"{msg.Role}: {msg.Content}");
            }
            
            // Add the current user message
            chatHistory.AddUserMessage(userMessage);
            conversationText.AppendLine($"user: {userMessage}");

            try
            {
                var function = _kernel.Plugins.GetFunction("AppointmentPlugin", "ExtractAppointmentDetails");
                var extractResult = await _kernel.InvokeAsync(function, new KernelArguments
                {
                    ["conversationText"] = conversationText.ToString()
                });
                
                string appointmentJson = extractResult.GetValue<string>();
                
                // Validate the extracted appointment details
                var validateFunction = _kernel.Plugins.GetFunction("AppointmentPlugin", "ValidateAppointment");
                var validationResult = await _kernel.InvokeAsync(validateFunction, new KernelArguments
                {
                    ["appointmentJson"] = appointmentJson
                });
                
                var validation = JsonSerializer.Deserialize<JsonElement>(validationResult.GetValue<string>());
                bool isValid = validation.GetProperty("isValid").GetBoolean();
                
                // If the appointment is valid, we can store this information for later
                // In a real app, you might store this in a session or database
            }
            catch (Exception ex)
            {
                // Handle exceptions but continue with the conversation
                Console.WriteLine($"Error processing appointment: {ex.Message}");
            }

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.7,
                TopP = 0.95,
                MaxTokens = 1000,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Get streaming response with tool calling enabled
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory: chatHistory,
                executionSettings: executionSettings,
                kernel: _kernel);

            var response = new StringBuilder();
            await foreach (var chunk in result)
            {
                response.Append(chunk);
            }

            return response.ToString();
        }

         // Method to directly check availability
        public async Task<string> CheckAvailabilityAsync(string specialty, string date = "")
        {
            var function = _kernel.Plugins.GetFunction("AppointmentPlugin", "CheckAvailability");
            var result = await _kernel.InvokeAsync(function, new KernelArguments
            {
                ["specialty"] = specialty,
                ["date"] = date
            });
            
            return result.GetValue<string>();
        }

        // Method to directly book an appointment
        public async Task<string> BookAppointmentAsync(AppointmentDetails appointment)
        {
            string appointmentJson = JsonSerializer.Serialize(appointment);
            
            var function = _kernel.Plugins.GetFunction("AppointmentPlugin", "BookAppointment");
            var result = await _kernel.InvokeAsync(function, new KernelArguments
            {
                ["appointmentJson"] = appointmentJson
            });
            
            return result.GetValue<string>();
        }
    }
}