using System;
using System.Collections.Generic;

namespace API.Contacts.Models
{
    /// <summary>
    /// Statistical information about messages in a conversation
    ///
    /// This class aggregates various metrics about messages for:
    /// - Analytics and reporting
    /// - Moderation purposes
    /// - Performance insights
    /// - User engagement tracking
    ///
    /// The statistics can be calculated on-demand or cached for
    /// frequently accessed conversations.
    /// </summary>
    public class MessageStatistics
    {
        /// <summary>
        /// Total number of messages in the conversation
        /// Includes all message types except deleted messages
        /// </summary>
        public int TotalMessages { get; set; }

        /// <summary>
        /// Timestamp of the first message sent
        /// Useful for calculating conversation age
        /// </summary>
        public DateTime? FirstMessageAt { get; set; }

        /// <summary>
        /// Timestamp of the most recent message
        /// Used for activity tracking
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Message count breakdown by user
        /// Key: User ID, Value: Number of messages sent by that user
        /// Useful for identifying conversation participants' activity levels
        /// </summary>
        public Dictionary<string, int> MessagesByUser { get; set; } = new();

        /// <summary>
        /// Message count breakdown by type
        /// Key: Message type (Text, Image, File, etc.), Value: Count
        /// Helps understand conversation composition
        /// </summary>
        public Dictionary<string, int> MessagesByType { get; set; } = new();

        /// <summary>
        /// Average length of text messages in characters
        /// Useful for gauging conversation depth
        /// </summary>
        public double AverageMessageLength { get; set; }

        /// <summary>
        /// Percentage of messages that are edited
        /// May indicate conversation quality or complexity
        /// </summary>
        public double EditedMessagePercentage { get; set; }

        /// <summary>
        /// Percentage of messages that are deleted
        /// May indicate moderation needs or user behavior patterns
        /// </summary>
        public double DeletedMessagePercentage { get; set; }

        /// <summary>
        /// Number of messages with attachments (files, images, etc.)
        /// </summary>
        public int MessagesWithAttachments { get; set; }

        /// <summary>
        /// Peak activity hour (0-23)
        /// Indicates when users are most active in this conversation
        /// </summary>
        public int? PeakActivityHour { get; set; }

        /// <summary>
        /// Average response time in seconds
        /// Time between consecutive messages from different users
        /// </summary>
        public double? AverageResponseTime { get; set; }

        /// <summary>
        /// Most active user ID
        /// User who has sent the most messages
        /// </summary>
        public string MostActiveUserId { get; set; }

        /// <summary>
        /// Number of messages sent in the last 24 hours
        /// Indicates recent conversation activity
        /// </summary>
        public int MessagesLast24Hours { get; set; }

        /// <summary>
        /// Number of messages sent in the last 7 days
        /// Indicates weekly conversation activity
        /// </summary>
        public int MessagesLast7Days { get; set; }

        /// <summary>
        /// Calculates derived statistics from the current data
        /// Should be called after populating basic statistics
        /// </summary>
        public void CalculateDerivedMetrics()
        {
            // Calculate most active user
            if (MessagesByUser != null && MessagesByUser.Count > 0)
            {
                var maxMessages = 0;
                foreach (var kvp in MessagesByUser)
                {
                    if (kvp.Value > maxMessages)
                    {
                        maxMessages = kvp.Value;
                        MostActiveUserId = kvp.Key;
                    }
                }
            }

            // Calculate edited percentage
            if (TotalMessages > 0 && MessagesByType.ContainsKey("Edited"))
            {
                EditedMessagePercentage = (double)MessagesByType["Edited"] / TotalMessages * 100;
            }

            // Calculate deleted percentage
            if (TotalMessages > 0 && MessagesByType.ContainsKey("Deleted"))
            {
                DeletedMessagePercentage = (double)MessagesByType["Deleted"] / TotalMessages * 100;
            }
        }

        /// <summary>
        /// Returns a summary string of key statistics
        /// Useful for logging and debugging
        /// </summary>
        public override string ToString()
        {
            return $"MessageStatistics: {TotalMessages} total messages, " +
                   $"{MessagesByUser?.Count ?? 0} participants, " +
                   $"Avg length: {AverageMessageLength:F1} chars, " +
                   $"Most active: {MostActiveUserId ?? "N/A"}";
        }
    }
}
