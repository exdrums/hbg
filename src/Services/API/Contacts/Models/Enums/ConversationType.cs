namespace API.Contacts.Models;

/// <summary>
/// Defines the type of conversation
/// This helps determine UI behavior and business rules
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Direct message between two users
    /// No title required, automatically shown as other participant's name
    /// </summary>
    Direct = 1,
    
    /// <summary>
    /// Group conversation with multiple participants
    /// Requires a title, supports adding/removing members
    /// </summary>
    Group = 2,
    
    /// <summary>
    /// System-generated conversation for announcements
    /// Read-only for regular users
    /// </summary>
    System = 3
}
