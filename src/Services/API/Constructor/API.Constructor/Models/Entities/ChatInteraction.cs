using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Constructor.Models.Entities
{
    [Table("ChatInteractions")]
    public class ChatInteraction
    {
        [Key]
        public Guid InteractionId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string UserMessage { get; set; }

        [MaxLength(4000)]
        public string AssistantResponse { get; set; }

        [Column(TypeName = "jsonb")]
        public string UpdatedConfigJson { get; set; }

        public Guid? ResultingImageId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual ConstructorProject Project { get; set; }

        [ForeignKey("ResultingImageId")]
        public virtual GeneratedImage ResultingImage { get; set; }

        public ChatInteraction()
        {
            InteractionId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
