using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data.Repositories;

public class ConversationParticipantRepository : RepositoryBase<ConversationParticipant>
{
    public ConversationParticipantRepository(DbContext context) : base(context)
    {
    }
}