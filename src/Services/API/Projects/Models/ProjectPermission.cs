using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Projects;

namespace Projects.Models;

[Table("ProjectPermission")]
public class ProjectPermission : IProjectPermission
{
    [Key]
    [Required]
    public long ProjectPermissionID { get; set; }

    [Required]
    public int ProjectID { get; set; }

    [Required]
    public string UserID { get; set; }
    
    [Required]
    public PermissionType Type { get; set; }

    [ForeignKey(nameof(ProjectID))]
    public Project Project { get; set; }
}