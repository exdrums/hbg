using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the authentication service for authorization checks
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public AuthenticationService(
            IConversationRepository conversationRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if a user is authorized to access a conversation
        /// </summary>
        public async Task<bool> IsUserAuthorizedForConversationAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                return false;
            }

            try
            {
                // Check if the user is a participant in the conversation
                return await _conversationRepository.IsUserParticipantAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is authorized for conversation {ConversationId}",
                    userId, conversationId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a user is authorized to add participants to a conversation
        /// </summary>
        public async Task<bool> CanAddParticipantsAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                return false;
            }

            try
            {
                // Get the conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    return false;
                }

                // For one-on-one conversations, both participants can add others (which converts it to a group)
                if (conversation.Type == ConversationType.OneOnOne)
                {
                    return await _conversationRepository.IsUserParticipantAsync(conversationId, userId);
                }

                // For AI assistant conversations, only the human participant can add others
                if (conversation.Type == ConversationType.AiAssistant)
                {
                    var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                    return participant != null && participant.Role != ParticipantRole.AiAssistant;
                }

                // For group conversations, only admins can add participants
                var groupParticipant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                return groupParticipant != null && groupParticipant.Role == ParticipantRole.Admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can add participants to conversation {ConversationId}",
                    userId, conversationId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a user is authorized to remove participants from a conversation
        /// </summary>
        public async Task<bool> CanRemoveParticipantsAsync(string userId, string conversationId, string participantToRemoveId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(participantToRemoveId))
            {
                return false;
            }

            try
            {
                // Cannot remove self (use "leave conversation" for that)
                if (userId == participantToRemoveId)
                {
                    return false;
                }

                // Get the conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    return false;
                }

                // Can't remove participants from one-on-one conversations
                if (conversation.Type == ConversationType.OneOnOne)
                {
                    return false;
                }

                // For AI assistant conversations, can't remove the AI
                if (conversation.Type == ConversationType.AiAssistant)
                {
                    var participantToRemove = conversation.Participants.FirstOrDefault(p => p.UserId == participantToRemoveId);
                    if (participantToRemove?.Role == ParticipantRole.AiAssistant)
                    {
                        return false;
                    }

                    // Only the creator can remove others
                    var requester = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                    return requester != null && requester.Role != ParticipantRole.AiAssistant;
                }

                // For group conversations, only admins can remove participants
                var groupParticipant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (groupParticipant == null || groupParticipant.Role != ParticipantRole.Admin)
                {
                    return false;
                }

                // Admins can't remove other admins unless they are the creator
                var participantToRemove = conversation.Participants.FirstOrDefault(p => p.UserId == participantToRemoveId);
                if (participantToRemove?.Role == ParticipantRole.Admin)
                {
                    // Need to check if the requester is the conversation creator
                    // For this simplified implementation, we'll consider the first admin as the creator
                    var creator = conversation.Participants
                        .Where(p => p.Role == ParticipantRole.Admin)
                        .OrderBy(p => p.JoinedAt)
                        .FirstOrDefault();

                    return creator?.UserId == userId;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can remove participant {ParticipantId} from conversation {ConversationId}",
                    userId, participantToRemoveId, conversationId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a user is authorized to update a conversation's properties
        /// </summary>
        public async Task<bool> CanUpdateConversationAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                return false;
            }

            try
            {
                // Get the conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    return false;
                }

                // For one-on-one and AI assistant conversations, any participant can update
                if (conversation.Type == ConversationType.OneOnOne || conversation.Type == ConversationType.AiAssistant)
                {
                    var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                    return participant != null && participant.Role != ParticipantRole.AiAssistant;
                }

                // For group conversations, only admins can update
                var groupParticipant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                return groupParticipant != null && groupParticipant.Role == ParticipantRole.Admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can update conversation {ConversationId}",
                    userId, conversationId);
                return false;
            }
        }

        /// <summary>
        /// Gets user information from an OIDC token
        /// </summary>
        public async Task<(string OidcSubject, string DisplayName)> GetUserFromOidcTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                // In a real implementation, this would validate the token and extract claims
                // For this simplified version, we'll just simulate extracting subject and name claims

                // Simulate a token format like "sub:displayName"
                var parts = token.Split(':');
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid token format");
                }

                var oidcSubject = parts[0];
                var displayName = parts[1];

                // Validate that the user exists or can be created
                var user = await _userRepository.GetByOidcSubjectAsync(oidcSubject);
                if (user == null)
                {
                    // In a real implementation, we might check additional authorization rules here
                    // before allowing a new user to be created
                }

                return (oidcSubject, displayName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user from OIDC token");
                throw;
            }
        }

        /// <summary>
        /// Validates an API key
        /// </summary>
        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            try
            {
                // In a real implementation, this would check the API key against stored keys
                // For this simplified version, we'll check against a configured key
                var validApiKey = _configuration["ApiKeys:DefaultKey"];

                return !string.IsNullOrEmpty(validApiKey) && apiKey == validApiKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return false;
            }
        }
    }
}
