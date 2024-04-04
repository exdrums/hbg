using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Project")]
public class Project 
{
    [Key]
    [Required]
    public int ProjectID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    [InverseProperty(nameof(ProjectPermission.Project))]
    public ICollection<ProjectPermission> ProjectPermissions { get; set; } = new List<ProjectPermission>();
}