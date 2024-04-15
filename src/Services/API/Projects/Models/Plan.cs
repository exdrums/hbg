using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projects.Models;

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



    [ForeignKey(nameof(ProjectID))]
    public Project Project { get; set; }

    [InverseProperty(nameof(Article.Plan))]
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
