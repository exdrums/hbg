using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google_GenerativeAI;

namespace API.Constructor.Services
{
    public class GeminiImageService : IGeminiImageService
    {
        private readonly GoogleAI _googleAi;
        private readonly ILogger<GeminiImageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _model;
        private readonly int _maxRetries;
        private readonly int _timeoutSeconds;

        public GeminiImageService(
            ILogger<GeminiImageService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var apiKey = _configuration["GeminiSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key not configured");
            }

            _googleAi = new GoogleAI(apiKey);
            _model = _configuration["GeminiSettings:Model"] ?? "imagen-3.0-generate-002";
            _maxRetries = int.Parse(_configuration["GeminiSettings:MaxRetries"] ?? "3");
            _timeoutSeconds = int.Parse(_configuration["GeminiSettings:TimeoutSeconds"] ?? "30");

            _logger.LogInformation("GeminiImageService initialized with model: {Model}", _model);
        }

        public async Task<GeneratedImageResult> GenerateImageAsync(string prompt, string aspectRatio = "1:1")
        {
            _logger.LogInformation("Generating image with prompt: {Prompt}, aspectRatio: {AspectRatio}", prompt, aspectRatio);

            var imageBytes = await GenerateImageBytesAsync(prompt, aspectRatio);

            return new GeneratedImageResult
            {
                ImageBytes = imageBytes,
                Prompt = prompt,
                AspectRatio = aspectRatio,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<byte[]> GenerateImageBytesAsync(string prompt, string aspectRatio = "1:1")
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < _maxRetries)
            {
                attempt++;
                try
                {
                    _logger.LogDebug("Image generation attempt {Attempt} of {MaxRetries}", attempt, _maxRetries);

                    var imageModel = _googleAi.CreateImageModel(_model);

                    var response = await imageModel.GenerateImagesAsync(
                        prompt: prompt,
                        numberOfImages: 1,
                        aspectRatio: aspectRatio
                    );

                    if (response == null || response.Images == null || !response.Images.Any())
                    {
                        throw new InvalidOperationException("Image generation returned no results");
                    }

                    var firstImage = response.Images.First();
                    var imageBytes = Convert.FromBase64String(firstImage.BytesBase64Encoded);

                    _logger.LogInformation("Image generated successfully. Size: {Size} bytes", imageBytes.Length);

                    return imageBytes;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Image generation attempt {Attempt} failed", attempt);

                    if (attempt < _maxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                        _logger.LogInformation("Retrying after {Delay} seconds...", delay.TotalSeconds);
                        await Task.Delay(delay);
                    }
                }
            }

            _logger.LogError(lastException, "Failed to generate image after {MaxRetries} attempts", _maxRetries);
            throw new InvalidOperationException($"Failed to generate image after {_maxRetries} attempts", lastException);
        }
    }
}
