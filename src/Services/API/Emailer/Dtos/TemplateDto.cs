namespace API.Emailer.Dtos;

/// <summary>
/// Dto for CRUD of the template with full content
/// </summary>
public class TemplateDto
{
    public long TemplateID { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }

}
