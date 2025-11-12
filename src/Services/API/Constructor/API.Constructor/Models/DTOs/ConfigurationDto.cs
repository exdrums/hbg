using System;
using System.Collections.Generic;

namespace API.Constructor.Models.DTOs
{
    public class ConfigurationDto
    {
        public Guid ConfigurationId { get; set; }
        public Guid ProjectId { get; set; }
        public string ConfigurationName { get; set; }
        public Dictionary<string, object> FormData { get; set; }
        public string GeneratedPrompt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateConfigurationDto
    {
        public Guid ProjectId { get; set; }
        public string ConfigurationName { get; set; }
        public Dictionary<string, object> FormData { get; set; }
    }

    public class UpdateConfigurationDto
    {
        public string ConfigurationName { get; set; }
        public Dictionary<string, object> FormData { get; set; }
    }
}
