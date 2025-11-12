using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Constructor.Models.Entities
{
    [Table("ProjectConfigurations")]
    public class ProjectConfiguration
    {
        [Key]
        public Guid ConfigurationId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ConfigurationName { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public string FormDataJson { get; set; }

        [MaxLength(2000)]
        public string GeneratedPrompt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual ConstructorProject Project { get; set; }

        public virtual ICollection<GeneratedImage> GeneratedImages { get; set; }

        public ProjectConfiguration()
        {
            ConfigurationId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            GeneratedImages = new HashSet<GeneratedImage>();
        }
    }
}
