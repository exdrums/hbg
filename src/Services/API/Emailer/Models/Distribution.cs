using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Distribution
{
    [Key]
    [Required]
    public long DistributionID { get; set; }

    [Required]
    public long SenderID { get; set; }

    [Required]
    public long TemplateID { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    public string Subject { get; set; }

    [ForeignKey(nameof(TemplateID))]
    public Template Template { get; set; }

    [ForeignKey(nameof(SenderID))]
    public Sender Sender { get; set; }

    [InverseProperty(nameof(Email.Distribution))]
    public ICollection<Email> Emails { get; set; }
    // public ICollection<Receiver> Receivers { get; set; }

    [NotMapped]
    public string TemplateName => Template.Name;

    [NotMapped]
    public string SenderName => Sender.Name;

    [NotMapped]
    public EmailStatus Status =>
        Emails.Any(e => e.Status == EmailStatus.Error) ? EmailStatus.Error : 
        Emails.Any(e => e.Status == EmailStatus.Pending) ? EmailStatus.Pending : 
        Emails.All(e => e.Status == EmailStatus.Sent) ? EmailStatus.Pending :
        EmailStatus.None;

    [NotMapped]
    public int EmailsCount => Emails.Count;
}
