using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projects.Models;

/// <summary>
/// Plan of one working room in the project,
/// the project can contain many plans where all articles placed in 
/// </summary>
public class Plan
{
    [Key]
    [Required]
    public int PlanID { get; set; }

    [Required]
    public int ProjectID { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public bool HasPlanPicture { get; set; }
    public decimal PicCenterX { get; set; }
    public decimal PicCenterY { get; set; }
    public decimal PicWidth { get; set; }
    public decimal PicHeight { get; set; }
    public decimal PicScale { get; set; }
    public decimal PicRotation { get; set; }

    
    [NotMapped]
    public bool IsReadOnly { get => false; }





    [ForeignKey(nameof(ProjectID))]
    public Project Project { get; set; }

    [InverseProperty(nameof(Article.Plan))]
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
