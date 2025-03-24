namespace API.Contacts.Model;

/// <summary>
/// User preferences stored as bit flags
/// </summary>
[Flags]
public enum UserPreferencesFlags
{
    None = 0,
    DarkMode = 1 << 0,
    NotificationsEnabled = 1 << 1,
    SoundEnabled = 1 << 2,
    AutoReadReceipts = 1 << 3,
    CompactView = 1 << 4,
    ShowTypingIndicators = 1 << 5,
    ShowPresenceStatus = 1 << 6,
    HighContrastMode = 1 << 7
}