using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;

namespace API.Constructor.Services
{
    public class FilesServiceClient : IFilesServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly ILogger<FilesServiceClient> _logger;

        public FilesServiceClient(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            ILogger<FilesServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("FilesService");
            _httpClient.BaseAddress = new Uri(appSettings.HBGFILES);
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<UploadFileResult> UploadImageAsync(byte[] imageBytes, string fileName, string projectId)
        {
            _logger.LogInformation("Uploading image {FileName} for project {ProjectId}", fileName, projectId);

            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(projectId), "projectId");

                var response = await _httpClient.PostAsync("/api/images/upload", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to upload image. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to upload image: {response.StatusCode}");
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UploadFileResult>(resultJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Image uploaded successfully. URL: {Url}", result?.Url);

                return result ?? new UploadFileResult
                {
                    FileId = Guid.NewGuid().ToString(),
                    Url = $"{_appSettings.HBGFILES}/images/{fileName}",
                    ThumbnailUrl = $"{_appSettings.HBGFILES}/images/thumbnails/{fileName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Files service");
                throw;
            }
        }

        public async Task<string> GetImageUrlAsync(string fileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/images/{fileId}");
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

                return result.GetProperty("url").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image URL from Files service");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string fileId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/images/{fileId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Files service");
                return false;
            }
        }
    }
}
