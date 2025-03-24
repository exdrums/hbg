using API.Contacts.Data.Repositories;
using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    // Custom DbSets using repository implementations
    public UserRepository Users { get; set; }
    public ConversationRepository Conversations { get; set; }
    public MessageRepository Messages { get; set; }
    public AlertRepository Alerts { get; set; }
    public ConversationParticipantRepository ConversationParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure composite key for ConversationParticipant
        modelBuilder.Entity<ConversationParticipant>()
            .HasKey(e => new { e.ConversationId, e.UserId });
    }
}
