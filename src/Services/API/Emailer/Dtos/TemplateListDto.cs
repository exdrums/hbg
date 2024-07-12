namespace API.Emailer.Dtos;

/// <summary>
/// Dto for template listing, without Content to improve list loading
/// </summary>
public class TemplateListDto
{
    public long TemplateID { get; set; }
    public string Name { get; set; }

    public int DistributionsCount { get; set; }
}
