using System.ComponentModel.DataAnnotations;

namespace Projects.Dtos;

public class PlanDto
{
    public int PlanID { get; set; }

    // public int ProjectID { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; }
}
