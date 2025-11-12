using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Constructor.Models.Enums;

namespace API.Constructor.Models.Entities
{
    [Table("ConstructorProjects")]
    public class ConstructorProject
    {
        [Key]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public JewelryType JewelryType { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<ProjectConfiguration> Configurations { get; set; }
        public virtual ICollection<ChatInteraction> ChatInteractions { get; set; }

        public ConstructorProject()
        {
            ProjectId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
            Configurations = new HashSet<ProjectConfiguration>();
            ChatInteractions = new HashSet<ChatInteraction>();
        }
    }
}
