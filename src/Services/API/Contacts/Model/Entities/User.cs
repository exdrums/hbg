using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Model;

/// <summary>
/// Represents a user in the chat system
/// </summary>
[Table("Users")]
[Index(nameof(OidcSubject), IsUnique = true)]
public class User
{
    /// <summary>
    /// Unique identifier for the user - usually mapped from OIDC
    /// </summary>
    [Key]
    [Required]
    [Column("Id", TypeName = "varchar(36)")]
    public string Id { get; private set; }

    /// <summary>
    /// User's display name in the chat
    /// </summary>
    [Required]
    [Column("Name", TypeName = "varchar(100)")]
    public string Name { get; private set; }

    /// <summary>
    /// URL to the user's avatar image
    /// </summary>
    [Column("AvatarUrl", TypeName = "varchar(500)")]
    public string AvatarUrl { get; private set; }

    /// <summary>
    /// Accessibility text for the user's avatar
    /// </summary>
    [Column("AvatarAlt", TypeName = "varchar(200)")]
    public string AvatarAlt { get; private set; }

    /// <summary>
    /// OIDC Subject identifier for authorization purposes
    /// </summary>
    [Required]
    [Column("OidcSubject", TypeName = "varchar(100)")]
    public string OidcSubject { get; private set; }

    /// <summary>
    /// Indicates if this user is an AI assistant
    /// </summary>
    [Required]
    [Column("IsAiAssistant", TypeName = "boolean")]
    public bool IsAiAssistant { get; private set; }

    /// <summary>
    /// Last time the user was active
    /// </summary>
    [Required]
    [Column("LastActive", TypeName = "timestamp with time zone")]
    public DateTime LastActive { get; private set; }

    /// <summary>
    /// User preferences as bit flags
    /// </summary>
    [Column("PreferencesValue", TypeName = "int")]
    public int PreferencesValue { get; private set; }
    
    [NotMapped]
    public UserPreferencesFlags Preferences 
    { 
        get => (UserPreferencesFlags)PreferencesValue;
        private set => PreferencesValue = (int)value;
    }

    public User(string id, string name, string oidcSubject, bool isAiAssistant = false)
    {
        Id = id;
        Name = name;
        OidcSubject = oidcSubject;
        IsAiAssistant = isAiAssistant;
        LastActive = DateTime.UtcNow;
        Preferences = UserPreferencesFlags.NotificationsEnabled | UserPreferencesFlags.SoundEnabled | UserPreferencesFlags.ShowTypingIndicators;
    }

    public void UpdateProfile(string name, string avatarUrl, string avatarAlt)
    {
        Name = name;
        AvatarUrl = avatarUrl;
        AvatarAlt = avatarAlt;
    }

    public void UpdateLastActive()
    {
        LastActive = DateTime.UtcNow;
    }
    
    public void SetPreference(UserPreferencesFlags preference, bool enabled)
    {
        if (enabled)
            Preferences |= preference;
        else
            Preferences &= ~preference;
    }
    
    public bool HasPreference(UserPreferencesFlags preference)
    {
        return (Preferences & preference) == preference;
    }
}