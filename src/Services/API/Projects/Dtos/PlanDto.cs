using System.ComponentModel.DataAnnotations;

namespace Projects.Dtos;

public class PlanDto
{
    public int PlanID { get; set; }
    public int ProjectID { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; }

    public bool HasPlanPicture { get; set; }
    public decimal PicCenterX { get; set; }
    public decimal PicCenterY { get; set; }
    public decimal PicWidth { get; set; }
    public decimal PicHeight { get; set; }
    public decimal PicScale { get; set; }
    public decimal PicRotation { get; set; }
    
    public bool IsReadOnly { get; set; }
}
