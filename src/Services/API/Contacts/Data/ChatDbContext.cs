using Microsoft.EntityFrameworkCore;
using API.Contacts.Models;

namespace API.Contacts.Data
{
    /// <summary>
    /// Entity Framework Core database context for the chat system
    /// 
    /// DbContext Design Principles:
    /// 1. Represents a session with the database
    /// 2. Manages entity tracking and change detection
    /// 3. Translates LINQ queries to SQL
    /// 4. Handles database connections and transactions
    /// 
    /// Configuration Strategy:
    /// - Using Data Annotations primarily (as requested)
    /// - Minimal Fluent API only for complex relationships
    /// - Composite keys defined where needed
    /// - Proper indexes for query performance
    /// 
    /// This context is registered as Scoped in DI container,
    /// meaning a new instance per request in web applications
    /// </summary>
    public class ChatDbContext : DbContext
    {
        /// <summary>
        /// Constructor accepting DbContextOptions for configuration
        /// This allows configuration to be injected from startup
        /// </summary>
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        #region DbSets - Tables in the Database

        /// <summary>
        /// Conversations table
        /// The main aggregate root for chat functionality
        /// </summary>
        public DbSet<Conversation> Conversations { get; set; }

        /// <summary>
        /// Messages table
        /// All messages across all conversations
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// ConversationParticipants join table
        /// Tracks which users belong to which conversations
        /// </summary>
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        /// <summary>
        /// MessageReadReceipts join table
        /// Tracks which users have read which messages
        /// </summary>
        public DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }

        #endregion

        /// <summary>
        /// Configure the model using Fluent API
        /// We use this minimally, only for things that can't be done with attributes
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary keys (can't be done with attributes)
            ConfigureConversationParticipants(modelBuilder);
            ConfigureMessageReadReceipts(modelBuilder);

            // Configure delete behavior
            ConfigureDeleteBehavior(modelBuilder);

            // Configure value conversions for enums (optional but recommended)
            ConfigureEnumConversions(modelBuilder);

            // Seed initial data if needed
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Configure the ConversationParticipants join table
        /// Composite key and relationships
        /// </summary>
        private void ConfigureConversationParticipants(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                // Composite primary key
                entity.HasKey(cp => new { cp.ConversationId, cp.UserId });

                // Relationship with Conversation
                entity.HasOne(cp => cp.Conversation)
                    .WithMany(c => c.Participants)
                    .HasForeignKey(cp => cp.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete participants when conversation is deleted

                // Default values
                entity.Property(cp => cp.Role)
                    .HasDefaultValue("Member");
                
                entity.Property(cp => cp.NotificationPreference)
                    .HasDefaultValue("All");

                entity.Property(cp => cp.UnreadCount)
                    .HasDefaultValue(0);
            });
        }

        /// <summary>
        /// Configure the MessageReadReceipts join table
        /// Composite key and relationships
        /// </summary>
        private void ConfigureMessageReadReceipts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageReadReceipt>(entity =>
            {
                // Composite primary key
                entity.HasKey(mr => new { mr.MessageId, mr.UserId });

                // Relationship with Message
                entity.HasOne(mr => mr.Message)
                    .WithMany(m => m.ReadReceipts)
                    .HasForeignKey(mr => mr.MessageId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete receipts when message is deleted
            });
        }

        /// <summary>
        /// Configure cascade delete behavior
        /// Important for maintaining referential integrity
        /// </summary>
        private void ConfigureDeleteBehavior(ModelBuilder modelBuilder)
        {
            // When a conversation is deleted, delete all its messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing relationship for message replies
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ReplyToMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.SetNull); // Don't cascade delete replies
        }

        /// <summary>
        /// Configure enum conversions for better database storage
        /// Stores enums as strings for readability in database
        /// </summary>
        private void ConfigureEnumConversions(ModelBuilder modelBuilder)
        {
            // Store ConversationType as string for clarity
            modelBuilder.Entity<Conversation>()
                .Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Store MessageType as string
            modelBuilder.Entity<Message>()
                .Property(m => m.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
        }

        /// <summary>
        /// Seed initial data for development/testing
        /// In production, this would be handled by migrations
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // // Example: Create a system conversation for announcements
            // var systemConversationId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            // modelBuilder.Entity<Conversation>().HasData(new Conversation
            // {
            //     ConversationId = systemConversationId,
            //     Title = "System Announcements",
            //     Type = ConversationType.System,
            //     CreatedByUserId = "SYSTEM",
            //     CreatedAt = DateTime.UtcNow,
            //     IsActive = true
            // });
        }

        /// <summary>
        /// Override SaveChanges to add automatic timestamps and audit fields
        /// This is a common pattern for tracking changes
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Async version of SaveChanges
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update timestamps on entities
        /// This could be extended to include audit fields (ModifiedBy, etc.)
        /// </summary>
        private void UpdateTimestamps()
        {
            // var entries = ChangeTracker.Entries()
            //     .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            // foreach (var entry in entries)
            // {
            //     // You could add interface-based timestamp handling here
            //     // For now, specific handling per entity type
                
            //     if (entry.Entity is Conversation conversation && entry.State == EntityState.Added)
            //     {
            //         conversation.CreatedAt = DateTime.UtcNow;
            //     }
                
            //     if (entry.Entity is Message message && entry.State == EntityState.Added)
            //     {
            //         message.SentAt = DateTime.UtcNow;
            //     }
            // }
        }
    }
}