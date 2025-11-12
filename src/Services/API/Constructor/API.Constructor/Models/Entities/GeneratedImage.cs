using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Constructor.Models.Enums;

namespace API.Constructor.Models.Entities
{
    [Table("GeneratedImages")]
    public class GeneratedImage
    {
        [Key]
        public Guid ImageId { get; set; }

        [Required]
        public Guid ConfigurationId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileServiceUrl { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(2000)]
        public string GenerationPrompt { get; set; }

        [Required]
        public GenerationSource GenerationSource { get; set; }

        [Required]
        [MaxLength(10)]
        public string AspectRatio { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        // Navigation properties
        [ForeignKey("ConfigurationId")]
        public virtual ProjectConfiguration Configuration { get; set; }

        public GeneratedImage()
        {
            ImageId = Guid.NewGuid();
            GeneratedAt = DateTime.UtcNow;
            IsDeleted = false;
            AspectRatio = "1:1";
        }
    }
}
