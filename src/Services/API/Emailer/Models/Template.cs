using System.ComponentModel.DataAnnotations.Schema;

namespace API.Emailer.Models;

public class Template
{
    public long TemplateID { get; set; }

    /// <summary>
    /// User id to authorize access to the template
    /// </summary>
    public string UserID { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }

    /// <summary>
    /// The template can be distributed many times
    /// </summary>
    [InverseProperty(nameof(Distribution.Template))]
    public ICollection<Distribution> Distributions { get; set; }
}