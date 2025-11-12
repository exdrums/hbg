using System;
using API.Constructor.Models.Enums;

namespace API.Constructor.Models.DTOs
{
    public class ImageDto
    {
        public Guid ImageId { get; set; }
        public Guid ConfigurationId { get; set; }
        public string FileServiceUrl { get; set; }
        public string FileName { get; set; }
        public string GenerationPrompt { get; set; }
        public GenerationSource GenerationSource { get; set; }
        public string AspectRatio { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class GenerateImageDto
    {
        public Guid ConfigurationId { get; set; }
        public string AspectRatio { get; set; } = "1:1";
    }
}
