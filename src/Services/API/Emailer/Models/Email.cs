using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Email
{
    [Key]
    [Required]
    public long EmailID { get; set; }

    [Required]
    public long DistributionID { get; set; }

    [Required]
    public long ReceiverID { get; set; }

    public EmailStatus Status { get; set; }

    [ForeignKey(nameof(DistributionID))]
    public Distribution Distribution { get; set; }

    [ForeignKey(nameof(ReceiverID))]
    public Receiver Receiver { get; set; }
}

public enum EmailStatus : byte
{
    None,
    Pending,
    Sent,
    Error
}