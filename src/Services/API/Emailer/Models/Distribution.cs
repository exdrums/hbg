namespace API.Emailer.Models;

public class Distribution
{
    public long DistributionID { get; set; }
    public long SenderID { get; set; }
    public long TemplateID { get; set; }

    public Template Template { get; set; }
    public Sender Sender { get; set; }
    public ICollection<Email> Emails { get; set; }
    // public ICollection<Receiver> Receivers { get; set; }
}
