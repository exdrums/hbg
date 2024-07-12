namespace API.Emailer.Models;

public class Receiver
{
    public long ReceiverID { get; set; }
    public long DistributionID { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }


    public Distribution Distribution { get; set; }

}
