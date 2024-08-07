using System.ComponentModel.DataAnnotations;
using API.Emailer.Models;

namespace API.Emailer.Dtos;

public class DistributionDto
{
    public long DistributionID { get; set; }
    public long SenderID { get; set; }
    public long TemplateID { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(100)]
    public string Subject { get; set; }
    
    [StringLength(100)]
    public string? SenderName { get; set; }
    
    [StringLength(100)]
    public string? TemplateName { get; set; }

    public EmailStatus Status { get; set; }
    public int EmailsCount { get; set; }
}
