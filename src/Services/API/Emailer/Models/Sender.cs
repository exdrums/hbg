namespace API.Emailer.Models;

public class Sender
{
    public long SenderID { get; set; }
    public string UserId { get; set; }
    public string Address { get; set; }

    public ICollection<Distribution> Distributions { get; set; }
    
}
