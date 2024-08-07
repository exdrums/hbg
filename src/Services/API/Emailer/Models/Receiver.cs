using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Receiver
{
    [Key]
    [Required]
    public long ReceiverID { get; set; }

    [Required]
    [StringLength(50)]
    public string UserID { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    public string Address { get; set; }

    [InverseProperty(nameof(Email.Receiver))]
    public ICollection<Email> Emails { get; set; }

}
