using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Sender
{
    [Key]
    [Required]
    public long SenderID { get; set; }

    [Required]
    [StringLength(50)]
    public string UserID { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Address { get; set; }

    [Required]
    [StringLength(100)]
    public string ServerAddress { get; set; }

    [Required]
    [StringLength(100)]
    public string Login { get; set; }

    [Required]
    [StringLength(100)]
    public string Passcode { get; set; }

    [InverseProperty(nameof(Distribution.Sender))]
    public ICollection<Distribution> Distributions { get; set; }
    
}
