using System.ComponentModel.DataAnnotations;

namespace API.Emailer.Dtos;

public class ReceiverDto
{
    public long ReceiverID { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    [Required]
    [StringLength(100)]
    public string Address { get; set; }

}
