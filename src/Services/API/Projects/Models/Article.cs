using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projects.Models;

[Table("Article")]
public class Article
{
    [Key]
    [Required]
    public long ArticleID { get; set; }

    [Required]
    public int ProjectID { get; set; }

    public int? PlanID { get; set; }


    [Required]
    [MaxLength(100)]
    public string Name { get; set; }



    [ForeignKey(nameof(ProjectID))]
    public Project Project { get; set; }

    [ForeignKey(nameof(PlanID))]
    public Plan Plan { get; set; }
}
