using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constructor.Models.Entities;
using API.Constructor.Models.Enums;

namespace API.Constructor.Services
{
    public interface IConfigurationService
    {
        string BuildPromptFromConfiguration(Dictionary<string, object> formData, JewelryType jewelryType);
        Task<ProjectConfiguration> SaveConfigurationAsync(Guid projectId, string configName, Dictionary<string, object> formData, JewelryType jewelryType);
        Task<GeneratedImage> GenerateAndSaveImageAsync(Guid configurationId, string aspectRatio = "1:1", GenerationSource source = GenerationSource.Form);
        Task<ProjectConfiguration> GetConfigurationAsync(Guid configurationId);
    }
}
