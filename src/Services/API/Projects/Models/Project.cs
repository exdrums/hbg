using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projects.Models;

/// <summary>
/// Model describing highest level of working entity
/// Contains all necessary informations about working project
/// </summary>
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

    public decimal DefaultCoorX { get; set; }
    public decimal DefaultCoorY { get; set; }

    [InverseProperty(nameof(ProjectPermission.Project))]
    public ICollection<ProjectPermission> ProjectPermissions { get; set; } = new List<ProjectPermission>();

    [InverseProperty(nameof(Article.Project))]
    public ICollection<Article> Articles { get; set; } = new List<Article>();

    [InverseProperty(nameof(Plan.Project))]
    public ICollection<Plan> Plans { get; set; } = new List<Plan>();
}