using System.ComponentModel.DataAnnotations;

namespace Projects.Dtos;

public class ArticleDto
{
    
    public long ArticleID { get; set; }

    // public int ProjectID { get; set; }

    // public int? PlanID { get; set; }


    [MaxLength(100)]
    public string Name { get; set; }
}
