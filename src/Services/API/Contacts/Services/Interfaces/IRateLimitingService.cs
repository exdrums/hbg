namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Rate limiting service
/// </summary>
public interface IRateLimitingService
{
    Task<bool> AllowRequestAsync(string userId, string operationType);
    Task<TimeSpan> GetTimeUntilNextAllowedRequestAsync(string userId, string operationType);
}
