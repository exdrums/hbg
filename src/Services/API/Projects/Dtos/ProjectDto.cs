using System.ComponentModel.DataAnnotations;

namespace Projects.Dtos;

public class ProjectDto
{
    [Required]
    public int ProjectID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}
