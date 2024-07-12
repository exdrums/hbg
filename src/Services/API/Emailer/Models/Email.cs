namespace API.Emailer.Models;

public class Email
{
    public long EmailID { get; set; }
    public long DistributionID { get; set; }
    public long ReceiverID { get; set; }
    public EmailStatus Status { get; set; }
    
    public Distribution Distribution { get; set; }
    public Receiver Receiver { get; set; }
}

public enum EmailStatus : byte
{
    None,
    Pending,
    Sent,
    Error
}