using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using API.Constructor.Data;
using API.Constructor.Models.Entities;
using API.Constructor.Models.Enums;

namespace API.Constructor.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ConstructorDbContext _context;
        private readonly IGeminiImageService _geminiService;
        private readonly IFilesServiceClient _filesClient;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(
            ConstructorDbContext context,
            IGeminiImageService geminiService,
            IFilesServiceClient filesClient,
            ILogger<ConfigurationService> logger)
        {
            _context = context;
            _geminiService = geminiService;
            _filesClient = filesClient;
            _logger = logger;
        }

        public string BuildPromptFromConfiguration(Dictionary<string, object> formData, JewelryType jewelryType)
        {
            var promptBuilder = new StringBuilder();

            // Base description
            promptBuilder.Append($"A hyperrealistic 3D render of a {jewelryType.ToString().ToLower()}, ");

            // Material
            if (formData.TryGetValue("material", out var material) && material != null)
            {
                promptBuilder.Append($"made of {material.ToString().ToLower()}, ");
            }

            // Gemstone
            if (formData.TryGetValue("gemstone", out var gemstone) && gemstone != null && gemstone.ToString() != "None")
            {
                promptBuilder.Append($"featuring {gemstone.ToString().ToLower()} gemstone, ");
            }

            // Style
            if (formData.TryGetValue("style", out var style) && style != null)
            {
                promptBuilder.Append($"{style.ToString().ToLower()} style, ");
            }

            // Finish
            if (formData.TryGetValue("finish", out var finish) && finish != null)
            {
                promptBuilder.Append($"{finish.ToString().ToLower()} finish, ");
            }

            // Additional notes
            if (formData.TryGetValue("notes", out var notes) && notes != null && !string.IsNullOrWhiteSpace(notes.ToString()))
            {
                promptBuilder.Append($"{notes.ToString()}, ");
            }

            // Quality modifiers
            promptBuilder.Append("high-quality product photography, studio lighting, white background, 4K, detailed craftsmanship");

            var prompt = promptBuilder.ToString();
            _logger.LogInformation("Built prompt: {Prompt}", prompt);

            return prompt;
        }

        public async Task<ProjectConfiguration> SaveConfigurationAsync(
            Guid projectId,
            string configName,
            Dictionary<string, object> formData,
            JewelryType jewelryType)
        {
            _logger.LogInformation("Saving configuration for project {ProjectId}", projectId);

            var prompt = BuildPromptFromConfiguration(formData, jewelryType);

            var configuration = new ProjectConfiguration
            {
                ProjectId = projectId,
                ConfigurationName = configName,
                FormDataJson = JsonSerializer.Serialize(formData),
                GeneratedPrompt = prompt
            };

            _context.ProjectConfigurations.Add(configuration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Configuration saved with ID: {ConfigurationId}", configuration.ConfigurationId);

            return configuration;
        }

        public async Task<GeneratedImage> GenerateAndSaveImageAsync(
            Guid configurationId,
            string aspectRatio = "1:1",
            GenerationSource source = GenerationSource.Form)
        {
            _logger.LogInformation("Generating image for configuration {ConfigurationId}", configurationId);

            var configuration = await _context.ProjectConfigurations
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.ConfigurationId == configurationId);

            if (configuration == null)
            {
                throw new InvalidOperationException($"Configuration {configurationId} not found");
            }

            // Generate image with Gemini
            var result = await _geminiService.GenerateImageAsync(configuration.GeneratedPrompt, aspectRatio);

            // Upload to Files service
            var fileName = $"jewelry_{configurationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var uploadResult = await _filesClient.UploadImageAsync(
                result.ImageBytes,
                fileName,
                configuration.ProjectId.ToString()
            );

            // Save image record
            var generatedImage = new GeneratedImage
            {
                ConfigurationId = configurationId,
                FileServiceUrl = uploadResult.Url,
                FileName = fileName,
                GenerationPrompt = configuration.GeneratedPrompt,
                GenerationSource = source,
                AspectRatio = aspectRatio,
                ThumbnailUrl = uploadResult.ThumbnailUrl
            };

            _context.GeneratedImages.Add(generatedImage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image generated and saved with ID: {ImageId}", generatedImage.ImageId);

            return generatedImage;
        }

        public async Task<ProjectConfiguration> GetConfigurationAsync(Guid configurationId)
        {
            return await _context.ProjectConfigurations
                .Include(c => c.Project)
                .Include(c => c.GeneratedImages)
                .FirstOrDefaultAsync(c => c.ConfigurationId == configurationId);
        }
    }
}
