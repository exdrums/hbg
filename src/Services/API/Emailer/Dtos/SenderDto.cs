using System.ComponentModel.DataAnnotations;

namespace API.Emailer.Dtos;

public class SenderDto
{
    public long SenderID { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(100)]
    public string ServerAddress { get; set; }

    [StringLength(100)]
    public string Login { get; set; }

    [StringLength(100)]
    public string Passcode { get; set; }
}
