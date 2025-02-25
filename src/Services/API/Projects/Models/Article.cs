using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Projects.Models.Enums;

namespace Projects.Models;

/// <summary>
/// First abstraction of one working item
/// </summary>
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

    public ArticleType Type { get; set; }

    public string TypeAsString => Type.ToString();

    public decimal? X { get; set; }
    public decimal? Y { get; set; }


    [ForeignKey(nameof(ProjectID))]
    public Project Project { get; set; }

    [ForeignKey(nameof(PlanID))]
    public Plan Plan { get; set; }
}
