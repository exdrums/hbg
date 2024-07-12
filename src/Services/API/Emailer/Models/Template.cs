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
    public ICollection<Distribution> Distributions { get; set; }
}