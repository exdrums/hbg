using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Receiver
{
    [Key]
    [Required]
    public long ReceiverID { get; set; }
    public long DistributionID { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }

    [ForeignKey(nameof(DistributionID))]
    public Distribution Distribution { get; set; }

}
