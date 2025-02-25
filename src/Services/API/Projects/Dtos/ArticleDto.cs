using System.ComponentModel.DataAnnotations;
using Projects.Models.Enums;

namespace Projects.Dtos;

public class ArticleDto
{
    
    public long ArticleID { get; set; }

    public int ProjectID { get; set; }

    public int? PlanID { get; set; }


    [MaxLength(100)]
    public string Name { get; set; }
    public ArticleType Type { get; set; }

    public string TypeAsString { get; set; }

    public decimal? X { get; set; }
    public decimal? Y { get; set; }
}
