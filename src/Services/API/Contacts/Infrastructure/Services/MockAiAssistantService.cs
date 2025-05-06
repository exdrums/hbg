using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Services
{
    /// <summary>
    /// Mock implementation of the AI assistant service for testing purposes
    /// </summary>
    public class MockAiAssistantService : IAiAssistantService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<MockAiAssistantService> _logger;
        private readonly AiAssistantOptions _options;

        // Simple in-memory request counter for rate limiting
        private readonly Dictionary<string, int> _userRequestCounts = new();
        private readonly object _lock = new();
        private DateTime _resetTime = DateTime.UtcNow.AddHours(1);

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public MockAiAssistantService(
            IUserRepository userRepository,
            IOptions<AiAssistantOptions> options,
            ILogger<MockAiAssistantService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets an AI response based on conversation history
        /// </summary>
        public async Task<string> GetAiResponseAsync(IEnumerable<Message> conversationHistory, string userId)
        {
            // Check request limit
            if (await IsRequestLimitReachedAsync(userId))
            {
                throw new InvalidOperationException("AI request limit reached. Please try again later.");
            }

            // Increment request count
            IncrementRequestCount(userId);

            try
            {
                await Task.Delay(1000); // Simulate AI processing time

                return GenerateMockResponse(conversationHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Regenerates an AI response for a specific message
        /// </summary>
        public async Task<string> RegenerateResponseAsync(IEnumerable<Message> conversationHistory, string messageId)
        {
            try
            {
                await Task.Delay(1500); // Simulate longer AI processing time for regeneration

                return "This is a regenerated response from the AI assistant. " +
                       "In a real implementation, this would use the conversation history to generate a coherent response " +
                       "that's different from the original.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI response for message {MessageId}", messageId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user has reached their request limit
        /// </summary>
        public Task<bool> IsRequestLimitReachedAsync(string userId)
        {
            lock (_lock)
            {
                // Reset counters if the reset time has passed
                if (DateTime.UtcNow > _resetTime)
                {
                    _userRequestCounts.Clear();
                    _resetTime = DateTime.UtcNow.AddHours(1);
                }

                // Check if the user has reached their limit
                if (_userRequestCounts.TryGetValue(userId, out var count))
                {
                    return Task.FromResult(count >= _options.MaxRequestsPerHour);
                }

                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Gets the system prompt for the AI assistant
        /// </summary>
        public Task<string> GetSystemPromptAsync()
        {
            return Task.FromResult(_options.SystemPrompt);
        }

        /// <summary>
        /// Creates an AI assistant user
        /// </summary>
        public async Task<User> CreateAiAssistantUserAsync()
        {
            // Check if the AI assistant user already exists
            var aiUser = await _userRepository.GetByOidcSubjectAsync("ai-assistant");

            if (aiUser != null)
            {
                return aiUser;
            }

            // Create a new AI assistant user
            aiUser = new User(
                "ai-assistant",
                "ai-assistant",
                "AI Assistant",
                "/images/ai-avatar.png",
                "AI Assistant Avatar"
            );

            await _userRepository.AddAsync(aiUser);

            return aiUser;
        }

        /// <summary>
        /// Gets information about API usage for a user
        /// </summary>
        public Task<(int Used, int Limit)> GetApiUsageInfoAsync(string userId)
        {
            lock (_lock)
            {
                int used = 0;

                if (_userRequestCounts.TryGetValue(userId, out var count))
                {
                    used = count;
                }

                return Task.FromResult((used, _options.MaxRequestsPerHour));
            }
        }

        /// <summary>
        /// Increments the request count for a user
        /// </summary>
        private void IncrementRequestCount(string userId)
        {
            lock (_lock)
            {
                // Reset counters if the reset time has passed
                if (DateTime.UtcNow > _resetTime)
                {
                    _userRequestCounts.Clear();
                    _resetTime = DateTime.UtcNow.AddHours(1);
                }

                // Increment the user's request count
                if (_userRequestCounts.TryGetValue(userId, out var count))
                {
                    _userRequestCounts[userId] = count + 1;
                }
                else
                {
                    _userRequestCounts[userId] = 1;
                }
            }
        }

        /// <summary>
        /// Generates a mock AI response based on conversation history
        /// </summary>
        private string GenerateMockResponse(IEnumerable<Message> conversationHistory)
        {
            // In a real implementation, this would use the conversation history
            // to generate a coherent response based on the context

            // For the mock service, we'll return a generic response
            var responses = new string[]
            {
                "I understand what you're saying. Could you tell me more about that?",
                "That's an interesting perspective. I've been thinking about this topic as well.",
                "Thank you for sharing that information. It helps me understand the situation better.",
                "I see your point. Let me think about how I can help with this.",
                "That's a great question. Based on what you've shared, I think we should consider several approaches.",
                "I appreciate your input on this matter. Let me suggest a few ideas that might help.",
                "I'm here to assist you with this. What specific aspect would you like me to focus on?",
                "I've analyzed what you've shared, and I think we should look at this from a different angle.",
                "Based on our conversation so far, I'd recommend taking the following steps...",
                "Let me summarize what we've discussed so far, and then I can offer some suggestions."
            };

            // Get a random response
            var random = new Random();
            return responses[random.Next(responses.Length)];
        }
    }
}
