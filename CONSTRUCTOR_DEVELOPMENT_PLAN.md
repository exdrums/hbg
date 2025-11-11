# Jewelry Constructor Microservice - Architecture & Development Plan

## Executive Summary

This document outlines the complete architecture and development plan for implementing a jewelry constructor microservice with AI-powered image generation capabilities using Google Gemini's Imagen-3.0 model.

---

## 1. Architecture Overview

### 1.1 System Components

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular SPA (Client)                     │
│  ┌────────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ DxForm         │  │ DxTabPanel   │  │ DxChat          │ │
│  │ (Configuration)│◄─┤ (Container)  ├─►│ (AI Assistant)  │ │
│  └────────┬───────┘  └──────────────┘  └────────┬────────┘ │
│           │                                       │          │
│           └───────────┬───────────────────────────┘          │
└───────────────────────┼──────────────────────────────────────┘
                        │ REST API + WebSocket (JWT)
                        ▼
┌─────────────────────────────────────────────────────────────┐
│              API.Constructor Microservice                    │
│  ┌──────────────────┐  ┌─────────────────────────────────┐ │
│  │ Controllers      │  │ SignalR Hub (ConstructorHub)    │ │
│  │ - Projects       │  │ - Chat Integration              │ │
│  │ - Configurations │  │ - Real-time Updates             │ │
│  │ - Images         │  └─────────────┬───────────────────┘ │
│  └────────┬─────────┘                │                     │
│           │                          │                     │
│  ┌────────▼──────────────────────────▼───────────────────┐ │
│  │           Services Layer                              │ │
│  │  ┌──────────────────┐  ┌─────────────────────────┐   │ │
│  │  │ ConfigService    │  │ GeminiImageService      │   │ │
│  │  │ - Validate forms │  │ - Generate images       │   │ │
│  │  │ - Build prompts  │  │ - Process responses     │   │ │
│  │  └──────────────────┘  └──────────┬──────────────┘   │ │
│  │  ┌──────────────────┐             │                   │ │
│  │  │ ProjectService   │             │                   │ │
│  │  │ - CRUD projects  │             │                   │ │
│  │  └──────────────────┘             │                   │ │
│  └───────────────────────────────────┼───────────────────┘ │
│                                      │                     │
│  ┌───────────────────────────────────▼───────────────────┐ │
│  │          Data Access Layer (EF Core)                  │ │
│  │  - ConstructorDbContext                               │ │
│  └───────────────────────────────────┬───────────────────┘ │
└────────────────────────────────────────┼───────────────────┘
                                         │
                    ┌────────────────────┼────────────────────┐
                    ▼                    ▼                    ▼
         ┌──────────────────┐  ┌─────────────────┐  ┌──────────────┐
         │ PostgreSQL DB    │  │ Files Service   │  │ Google       │
         │ (Projects Data)  │  │ (Image Storage) │  │ Gemini API   │
         └──────────────────┘  └─────────────────┘  └──────────────┘
```

### 1.2 Technology Stack

**Backend:**
- ASP.NET Core 6.0+
- Entity Framework Core 7.0+ (PostgreSQL provider)
- Google_GenerativeAI SDK 3.4.0+ (NuGet)
- SignalR for real-time communication
- AutoMapper for DTO mapping
- Serilog for logging

**Frontend:**
- Angular 18.2.12
- DevExtreme 24.2.5+ (using new configuration components)
- @microsoft/signalr 6.0.1
- RxJS 7.8.1
- TypeScript 5.5.2

**Infrastructure:**
- Kubernetes (k8s)
- PostgreSQL 14+
- Docker
- NGINX Ingress Controller

---

## 2. Database Schema Design

### 2.1 Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     ConstructorProject                       │
├─────────────────────────────────────────────────────────────┤
│ ProjectId (PK)              : Guid                          │
│ UserId                      : string                        │
│ Name                        : string (100)                  │
│ Description                 : string (500)                  │
│ JewelryType                 : enum (Ring, Earring, etc.)    │
│ CreatedAt                   : DateTime                      │
│ UpdatedAt                   : DateTime                      │
│ IsActive                    : bool                          │
└──────────────────┬──────────────────────────────────────────┘
                   │ 1
                   │
                   │ *
┌──────────────────▼──────────────────────────────────────────┐
│                 ProjectConfiguration                         │
├─────────────────────────────────────────────────────────────┤
│ ConfigurationId (PK)        : Guid                          │
│ ProjectId (FK)              : Guid                          │
│ ConfigurationName           : string (100)                  │
│ FormDataJson                : jsonb                         │
│ GeneratedPrompt             : string (2000)                 │
│ CreatedAt                   : DateTime                      │
│ UpdatedAt                   : DateTime                      │
└──────────────────┬──────────────────────────────────────────┘
                   │ 1
                   │
                   │ *
┌──────────────────▼──────────────────────────────────────────┐
│                   GeneratedImage                             │
├─────────────────────────────────────────────────────────────┤
│ ImageId (PK)                : Guid                          │
│ ConfigurationId (FK)        : Guid                          │
│ FileServiceUrl              : string (500)                  │
│ FileName                    : string (255)                  │
│ GenerationPrompt            : string (2000)                 │
│ GenerationSource            : enum (Form, Chat)             │
│ AspectRatio                 : string (10)                   │
│ GeneratedAt                 : DateTime                      │
│ ThumbnailUrl                : string (500)                  │
│ IsDeleted                   : bool                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                     ChatInteraction                          │
├─────────────────────────────────────────────────────────────┤
│ InteractionId (PK)          : Guid                          │
│ ProjectId (FK)              : Guid                          │
│ UserId                      : string                        │
│ UserMessage                 : string (2000)                 │
│ AssistantResponse           : string (4000)                 │
│ UpdatedConfigJson           : jsonb                         │
│ ResultingImageId (FK)       : Guid (nullable)               │
│ CreatedAt                   : DateTime                      │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Table Definitions

#### ConstructorProject
```sql
CREATE TABLE "ConstructorProjects" (
    "ProjectId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "JewelryType" SMALLINT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT "FK_ConstructorProjects_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ConstructorProjects_UserId" ON "ConstructorProjects"("UserId");
CREATE INDEX "IX_ConstructorProjects_CreatedAt" ON "ConstructorProjects"("CreatedAt" DESC);
```

#### ProjectConfiguration
```sql
CREATE TABLE "ProjectConfigurations" (
    "ConfigurationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "ConfigurationName" VARCHAR(100) NOT NULL,
    "FormDataJson" JSONB NOT NULL,
    "GeneratedPrompt" VARCHAR(2000),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "FK_ProjectConfigurations_Projects"
        FOREIGN KEY ("ProjectId") REFERENCES "ConstructorProjects"("ProjectId") ON DELETE CASCADE
);

CREATE INDEX "IX_ProjectConfigurations_ProjectId" ON "ProjectConfigurations"("ProjectId");
```

#### GeneratedImage
```sql
CREATE TABLE "GeneratedImages" (
    "ImageId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ConfigurationId" UUID NOT NULL,
    "FileServiceUrl" VARCHAR(500) NOT NULL,
    "FileName" VARCHAR(255) NOT NULL,
    "GenerationPrompt" VARCHAR(2000) NOT NULL,
    "GenerationSource" SMALLINT NOT NULL,
    "AspectRatio" VARCHAR(10) NOT NULL DEFAULT '1:1',
    "GeneratedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ThumbnailUrl" VARCHAR(500),
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT "FK_GeneratedImages_Configurations"
        FOREIGN KEY ("ConfigurationId") REFERENCES "ProjectConfigurations"("ConfigurationId") ON DELETE CASCADE
);

CREATE INDEX "IX_GeneratedImages_ConfigurationId" ON "GeneratedImages"("ConfigurationId");
CREATE INDEX "IX_GeneratedImages_GeneratedAt" ON "GeneratedImages"("GeneratedAt" DESC);
```

#### ChatInteraction
```sql
CREATE TABLE "ChatInteractions" (
    "InteractionId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "UserId" VARCHAR(450) NOT NULL,
    "UserMessage" VARCHAR(2000) NOT NULL,
    "AssistantResponse" VARCHAR(4000),
    "UpdatedConfigJson" JSONB,
    "ResultingImageId" UUID,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "FK_ChatInteractions_Projects"
        FOREIGN KEY ("ProjectId") REFERENCES "ConstructorProjects"("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChatInteractions_Images"
        FOREIGN KEY ("ResultingImageId") REFERENCES "GeneratedImages"("ImageId") ON DELETE SET NULL
);

CREATE INDEX "IX_ChatInteractions_ProjectId" ON "ChatInteractions"("ProjectId");
CREATE INDEX "IX_ChatInteractions_CreatedAt" ON "ChatInteractions"("CreatedAt" DESC);
```

### 2.3 Enums

```csharp
public enum JewelryType : byte
{
    Ring = 1,
    Earring = 2,
    Necklace = 3,
    Bracelet = 4,
    Piercing = 5,
    Pendant = 6,
    Brooch = 7,
    Anklet = 8,
    Other = 99
}

public enum GenerationSource : byte
{
    Form = 1,
    Chat = 2
}
```

---

## 3. Backend Implementation Plan

### 3.1 Project Structure

```
src/Services/API/Constructor/
├── API.Constructor/
│   ├── Controllers/
│   │   ├── ProjectsController.cs
│   │   ├── ConfigurationsController.cs
│   │   ├── ImagesController.cs
│   │   └── ValuesController.cs
│   ├── WebSocket/
│   │   └── ConstructorHub.cs
│   ├── Services/
│   │   ├── IProjectService.cs
│   │   ├── ProjectService.cs
│   │   ├── IConfigurationService.cs
│   │   ├── ConfigurationService.cs
│   │   ├── IGeminiImageService.cs
│   │   ├── GeminiImageService.cs
│   │   ├── IFilesServiceClient.cs
│   │   ├── FilesServiceClient.cs
│   │   └── AuthService.cs
│   ├── Data/
│   │   ├── ConstructorDbContext.cs
│   │   └── Migrations/
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── ConstructorProject.cs
│   │   │   ├── ProjectConfiguration.cs
│   │   │   ├── GeneratedImage.cs
│   │   │   └── ChatInteraction.cs
│   │   ├── DTOs/
│   │   │   ├── ProjectDto.cs
│   │   │   ├── ConfigurationDto.cs
│   │   │   ├── ImageDto.cs
│   │   │   └── ChatMessageDto.cs
│   │   └── Enums/
│   │       ├── JewelryType.cs
│   │       └── GenerationSource.cs
│   ├── Mapping/
│   │   └── MappingProfile.cs
│   ├── Program.cs
│   ├── Services.cs
│   ├── Pipeline.cs
│   └── appsettings.json
└── API.Constructor.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

### 3.2 Key Service Implementations

#### 3.2.1 GeminiImageService.cs

**Purpose:** Generate images using Google Gemini Imagen-3.0 model

**Dependencies:**
- NuGet: `Google_GenerativeAI` (v3.4.0+)
- Configuration: API Key, Model settings

**Implementation Pattern:**

```csharp
public interface IGeminiImageService
{
    Task<GeneratedImageResult> GenerateImageAsync(string prompt, string aspectRatio = "1:1");
    Task<byte[]> GenerateImageBytesAsync(string prompt, string aspectRatio = "1:1");
}

public class GeminiImageService : IGeminiImageService
{
    private readonly GoogleAI _googleAi;
    private readonly ILogger<GeminiImageService> _logger;
    private readonly IConfiguration _configuration;

    public async Task<GeneratedImageResult> GenerateImageAsync(string prompt, string aspectRatio = "1:1")
    {
        try
        {
            var imageModel = _googleAi.CreateImageModel("imagen-3.0-generate-002");

            var response = await imageModel.GenerateImagesAsync(
                prompt: prompt,
                numberOfImages: 1,
                aspectRatio: aspectRatio
            );

            var imageBytes = Convert.FromBase64String(
                response.Images.First().BytesBase64Encoded
            );

            return new GeneratedImageResult
            {
                ImageBytes = imageBytes,
                Prompt = prompt,
                AspectRatio = aspectRatio,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image with prompt: {Prompt}", prompt);
            throw;
        }
    }
}
```

#### 3.2.2 FilesServiceClient.cs

**Purpose:** Upload and retrieve images from Files microservice

**Implementation Pattern:**

```csharp
public interface IFilesServiceClient
{
    Task<UploadFileResult> UploadImageAsync(byte[] imageBytes, string fileName, string projectId);
    Task<string> GetImageUrlAsync(string fileId);
    Task<bool> DeleteImageAsync(string fileId);
}

public class FilesServiceClient : IFilesServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;

    public FilesServiceClient(IHttpClientFactory httpClientFactory, AppSettings appSettings)
    {
        _httpClient = httpClientFactory.CreateClient("FilesService");
        _httpClient.BaseAddress = new Uri(appSettings.HBGFILES);
        _appSettings = appSettings;
    }

    public async Task<UploadFileResult> UploadImageAsync(byte[] imageBytes, string fileName, string projectId)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(projectId), "projectId");

        var response = await _httpClient.PostAsync("/api/images/upload", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UploadFileResult>();
        return result;
    }
}
```

#### 3.2.3 ConfigurationService.cs

**Purpose:** Build AI prompts from form configurations

**Implementation Pattern:**

```csharp
public interface IConfigurationService
{
    string BuildPromptFromConfiguration(Dictionary<string, object> formData, JewelryType jewelryType);
    Task<ProjectConfiguration> SaveConfigurationAsync(Guid projectId, Dictionary<string, object> formData);
    Task<GeneratedImage> GenerateAndSaveImageAsync(Guid configurationId);
}

public class ConfigurationService : IConfigurationService
{
    public string BuildPromptFromConfiguration(Dictionary<string, object> formData, JewelryType jewelryType)
    {
        var promptBuilder = new StringBuilder();

        // Base description
        promptBuilder.Append($"A hyperrealistic 3D render of a {jewelryType.ToString().ToLower()}, ");

        // Material
        if (formData.TryGetValue("material", out var material))
        {
            promptBuilder.Append($"made of {material}, ");
        }

        // Gemstone
        if (formData.TryGetValue("gemstone", out var gemstone) && gemstone?.ToString() != "None")
        {
            promptBuilder.Append($"featuring {gemstone} gemstone, ");
        }

        // Style
        if (formData.TryGetValue("style", out var style))
        {
            promptBuilder.Append($"{style} style, ");
        }

        // Finish
        if (formData.TryGetValue("finish", out var finish))
        {
            promptBuilder.Append($"{finish} finish, ");
        }

        // Quality modifiers
        promptBuilder.Append("high-quality product photography, studio lighting, white background, 4K, detailed craftsmanship");

        return promptBuilder.ToString();
    }
}
```

### 3.3 SignalR Hub Implementation

#### ConstructorHub.cs

**Purpose:** Real-time updates for chat-based image generation

```csharp
[Authorize]
public class ConstructorHub : Hub
{
    private readonly IConfigurationService _configService;
    private readonly IGeminiImageService _geminiService;
    private readonly IProjectService _projectService;

    public async Task SendChatMessage(Guid projectId, string message)
    {
        var userId = Context.User.FindFirst("sub")?.Value;

        // Process user message and update configuration
        var updatedConfig = await ProcessChatMessageAsync(projectId, message);

        // Generate new image
        var image = await _configService.GenerateAndSaveImageAsync(updatedConfig.ConfigurationId);

        // Send response to user
        await Clients.User(userId).SendAsync("ReceiveImageUpdate", new
        {
            ImageUrl = image.FileServiceUrl,
            Configuration = updatedConfig,
            Message = "Image generated successfully based on your request"
        });
    }

    public async Task RegenerateImage(Guid configurationId)
    {
        var userId = Context.User.FindFirst("sub")?.Value;
        var image = await _configService.GenerateAndSaveImageAsync(configurationId);

        await Clients.User(userId).SendAsync("ReceiveImageUpdate", new
        {
            ImageUrl = image.FileServiceUrl
        });
    }
}
```

### 3.4 Authentication & Authorization

**Pattern:** JWT Bearer tokens from Identity Server

```csharp
// Services.cs
services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = appSettings.HBGIDENTITY;
        options.RequireHttpsMetadata = false;
        options.Audience = "api_constructor";

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/constructor"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
```

### 3.5 Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "HBGCONSTRUCTORDB": "Server=localhost;port=32345;Database=hbgconstructordb;Uid=hbg-dbuser;Pwd=hbg-password-database",
  "HBGCONSTRUCTOR": "http://localhost:5705",
  "HBGIDENTITY": "https://localhost:5700",
  "HBGFILES": "http://localhost:5701",
  "HBGSPA": "https://localhost:5799",
  "HBGSPADEV": "http://localhost:4200",
  "AUDIENCE": "api_constructor",
  "EnableSeeding": false,

  "GeminiSettings": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "imagen-3.0-generate-002",
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "DefaultAspectRatio": "1:1",
    "SupportedAspectRatios": ["1:1", "3:4", "4:3", "9:16", "16:9"]
  },

  "ImageGenerationSettings": {
    "MaxImagesPerProject": 100,
    "MaxImageSizeMB": 10,
    "DefaultImageQuality": "high",
    "ConcurrentGenerations": 3
  }
}
```

---

## 4. Files Service Enhancement

### 4.1 Required Changes

**New Controller:** `ImagesController.cs`

```csharp
[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImageStorageService _imageStorage;
    private readonly IAuthService _authService;

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string projectId)
    {
        var userId = _authService.GetCurrentUserId();

        // Validate file
        if (!IsValidImageFile(file))
            return BadRequest("Invalid image file");

        // Store image
        var result = await _imageStorage.StoreImageAsync(file, userId, projectId);

        return Ok(new
        {
            FileId = result.FileId,
            Url = result.Url,
            ThumbnailUrl = result.ThumbnailUrl
        });
    }

    [HttpGet("{fileId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(string fileId)
    {
        var image = await _imageStorage.GetImageAsync(fileId);
        if (image == null)
            return NotFound();

        return File(image.Bytes, image.ContentType);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteImage(string fileId)
    {
        var userId = _authService.GetCurrentUserId();
        await _imageStorage.DeleteImageAsync(fileId, userId);
        return NoContent();
    }
}
```

**Storage Structure:**

```
/files/
  /constructor/
    /{userId}/
      /{projectId}/
        /images/
          /{imageId}.jpg
        /thumbnails/
          /{imageId}_thumb.jpg
```

### 4.2 Image Processing Service

**Features:**
- Resize and optimize images
- Generate thumbnails
- Convert formats (PNG → JPEG)
- Add watermarks (optional)

**Recommended NuGet:** `SixLabors.ImageSharp` (v3.0+)

---

## 5. Frontend Implementation Plan

### 5.1 Module Structure

```
src/app/modules/constructor/
├── constructor.module.ts
├── constructor.routes.ts
├── components/
│   ├── constructor-main/
│   │   ├── constructor-main.component.ts
│   │   ├── constructor-main.component.html
│   │   └── constructor-main.component.scss
│   ├── configuration-form/
│   │   ├── configuration-form.component.ts
│   │   ├── configuration-form.component.html
│   │   └── configuration-form.component.scss
│   ├── chat-assistant/
│   │   ├── chat-assistant.component.ts
│   │   ├── chat-assistant.component.html
│   │   └── chat-assistant.component.scss
│   ├── image-gallery/
│   │   ├── image-gallery.component.ts
│   │   ├── image-gallery.component.html
│   │   └── image-gallery.component.scss
│   └── project-list/
│       ├── project-list.component.ts
│       ├── project-list.component.html
│       └── project-list.component.scss
├── services/
│   ├── constructor.service.ts
│   ├── constructor-hub.service.ts
│   └── image-cache.service.ts
├── models/
│   ├── project.model.ts
│   ├── configuration.model.ts
│   ├── generated-image.model.ts
│   └── jewelry-type.enum.ts
└── data/
    ├── projects.data-source.ts
    └── images.data-source.ts
```

### 5.2 Component Implementations

#### 5.2.1 Constructor Main Component

**Purpose:** Container with DxTabPanel for Form and Chat tabs

**Template (constructor-main.component.html):**

```html
<div class="constructor-container">
  <div class="header">
    <h2>{{ currentProject?.name || 'Jewelry Constructor' }}</h2>
    <dx-button
      text="New Project"
      icon="add"
      (onClick)="createNewProject()">
    </dx-button>
  </div>

  <div class="content-layout">
    <!-- Left Panel: Tab Panel with Form and Chat -->
    <div class="control-panel">
      <dx-tab-panel
        [dataSource]="tabs"
        [selectedIndex]="selectedTabIndex"
        (onSelectionChanged)="onTabChanged($event)"
        [animationEnabled]="true"
        [swipeEnabled]="false">

        <!-- Configuration Tab -->
        <dxi-item title="Configuration" icon="preferences">
          <div *dxTemplate>
            <app-configuration-form
              [projectId]="currentProject?.projectId"
              [jewelryType]="currentProject?.jewelryType"
              (onConfigurationChanged)="handleConfigurationChange($event)"
              (onGenerateImage)="generateImage($event)">
            </app-configuration-form>
          </div>
        </dxi-item>

        <!-- Chat Assistant Tab -->
        <dxi-item title="AI Assistant" icon="comment">
          <div *dxTemplate>
            <app-chat-assistant
              [projectId]="currentProject?.projectId"
              (onMessageSent)="handleChatMessage($event)">
            </app-chat-assistant>
          </div>
        </dxi-item>
      </dx-tab-panel>
    </div>

    <!-- Right Panel: Image Gallery -->
    <div class="gallery-panel">
      <app-image-gallery
        [projectId]="currentProject?.projectId"
        [images]="generatedImages"
        [loading]="isGenerating">
      </app-image-gallery>
    </div>
  </div>
</div>
```

**Component (constructor-main.component.ts):**

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ConstructorService } from '../../services/constructor.service';
import { ConstructorHubService } from '../../services/constructor-hub.service';
import { Project, Configuration, GeneratedImage } from '../../models';

@Component({
  selector: 'app-constructor-main',
  templateUrl: './constructor-main.component.html',
  styleUrls: ['./constructor-main.component.scss']
})
export class ConstructorMainComponent implements OnInit, OnDestroy {
  currentProject: Project | null = null;
  generatedImages: GeneratedImage[] = [];
  selectedTabIndex = 0;
  isGenerating = false;

  tabs = [
    { title: 'Configuration', icon: 'preferences' },
    { title: 'AI Assistant', icon: 'comment' }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private constructorService: ConstructorService,
    private hubService: ConstructorHubService
  ) {}

  async ngOnInit() {
    // Subscribe to real-time updates
    this.hubService.onImageUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe(image => {
        this.generatedImages.unshift(image);
        this.isGenerating = false;
      });

    // Load project from route
    const projectId = this.route.snapshot.params['id'];
    if (projectId) {
      await this.loadProject(projectId);
    }
  }

  async loadProject(projectId: string) {
    this.currentProject = await this.constructorService.getProject(projectId);
    this.generatedImages = await this.constructorService.getProjectImages(projectId);
  }

  async handleConfigurationChange(config: Configuration) {
    if (this.currentProject) {
      await this.constructorService.saveConfiguration(
        this.currentProject.projectId,
        config
      );
    }
  }

  async generateImage(config: Configuration) {
    this.isGenerating = true;
    try {
      await this.constructorService.generateImage(config.configurationId);
    } catch (error) {
      console.error('Image generation failed:', error);
      this.isGenerating = false;
    }
  }

  async handleChatMessage(message: string) {
    if (this.currentProject) {
      this.isGenerating = true;
      await this.hubService.sendChatMessage(this.currentProject.projectId, message);
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

#### 5.2.2 Configuration Form Component

**Purpose:** DxForm for jewelry configuration

**Template (configuration-form.component.html):**

```html
<dx-form
  #form
  [formData]="formData"
  [colCount]="2"
  labelLocation="top"
  (onFieldDataChanged)="onFieldChanged($event)">

  <!-- Jewelry Type -->
  <dxi-item
    dataField="jewelryType"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: jewelryTypes,
      displayExpr: 'name',
      valueExpr: 'value',
      searchEnabled: true
    }">
    <dxo-label text="Jewelry Type"></dxo-label>
    <dxi-validation-rule type="required" message="Jewelry type is required"></dxi-validation-rule>
  </dxi-item>

  <!-- Material -->
  <dxi-item
    dataField="material"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: materials,
      searchEnabled: true
    }">
    <dxo-label text="Material"></dxo-label>
    <dxi-validation-rule type="required"></dxi-validation-rule>
  </dxi-item>

  <!-- Gemstone -->
  <dxi-item
    dataField="gemstone"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: gemstones,
      searchEnabled: true
    }">
    <dxo-label text="Gemstone"></dxo-label>
  </dxi-item>

  <!-- Style -->
  <dxi-item
    dataField="style"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: styles
    }">
    <dxo-label text="Style"></dxo-label>
  </dxi-item>

  <!-- Finish -->
  <dxi-item
    dataField="finish"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: finishes
    }">
    <dxo-label text="Finish"></dxo-label>
  </dxi-item>

  <!-- Aspect Ratio -->
  <dxi-item
    dataField="aspectRatio"
    editorType="dxSelectBox"
    [editorOptions]="{
      items: aspectRatios
    }">
    <dxo-label text="Image Aspect Ratio"></dxo-label>
  </dxi-item>

  <!-- Additional Notes -->
  <dxi-item
    dataField="notes"
    editorType="dxTextArea"
    [colSpan]="2"
    [editorOptions]="{
      height: 100,
      maxLength: 500
    }">
    <dxo-label text="Additional Details"></dxo-label>
  </dxi-item>

  <!-- Actions -->
  <dxi-item
    itemType="button"
    [colSpan]="2"
    [buttonOptions]="{
      text: 'Generate Image',
      type: 'default',
      icon: 'image',
      onClick: onGenerateClick.bind(this)
    }">
  </dxi-item>
</dx-form>
```

**Component (configuration-form.component.ts):**

```typescript
import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { DxFormComponent } from 'devextreme-angular';

@Component({
  selector: 'app-configuration-form',
  templateUrl: './configuration-form.component.html',
  styleUrls: ['./configuration-form.component.scss']
})
export class ConfigurationFormComponent {
  @Input() projectId: string | null = null;
  @Input() jewelryType: number | null = null;
  @Output() onConfigurationChanged = new EventEmitter<any>();
  @Output() onGenerateImage = new EventEmitter<any>();

  @ViewChild('form', { static: false }) form!: DxFormComponent;

  formData: any = {
    jewelryType: null,
    material: 'Gold',
    gemstone: 'None',
    style: 'Classic',
    finish: 'Polished',
    aspectRatio: '1:1',
    notes: ''
  };

  jewelryTypes = [
    { name: 'Ring', value: 1 },
    { name: 'Earring', value: 2 },
    { name: 'Necklace', value: 3 },
    { name: 'Bracelet', value: 4 },
    { name: 'Piercing', value: 5 },
    { name: 'Pendant', value: 6 }
  ];

  materials = [
    'Gold', 'White Gold', 'Rose Gold', 'Platinum',
    'Silver', 'Titanium', 'Stainless Steel'
  ];

  gemstones = [
    'None', 'Diamond', 'Ruby', 'Sapphire', 'Emerald',
    'Amethyst', 'Topaz', 'Opal', 'Pearl'
  ];

  styles = [
    'Classic', 'Modern', 'Vintage', 'Bohemian',
    'Minimalist', 'Art Deco', 'Victorian'
  ];

  finishes = [
    'Polished', 'Matte', 'Brushed', 'Hammered', 'Antiqued'
  ];

  aspectRatios = ['1:1', '3:4', '4:3', '9:16', '16:9'];

  onFieldChanged(e: any) {
    this.onConfigurationChanged.emit(this.formData);
  }

  onGenerateClick() {
    const result = this.form.instance.validate();
    if (result.isValid) {
      this.onGenerateImage.emit(this.formData);
    }
  }
}
```

#### 5.2.3 Chat Assistant Component

**Purpose:** DxChat for AI-powered modifications

**Template (chat-assistant.component.html):**

```html
<dx-chat
  [user]="currentUser"
  [items]="messages"
  (onMessageEntered)="onMessageEntered($event)"
  height="600px">

  <dxo-user
    [id]="currentUser.id"
    [name]="currentUser.name">
  </dxo-user>
</dx-chat>
```

**Component (chat-assistant.component.ts):**

```typescript
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-chat-assistant',
  templateUrl: './chat-assistant.component.html',
  styleUrls: ['./chat-assistant.component.scss']
})
export class ChatAssistantComponent {
  @Input() projectId: string | null = null;
  @Output() onMessageSent = new EventEmitter<string>();

  currentUser = {
    id: 'user-1',
    name: 'You'
  };

  aiUser = {
    id: 'ai-assistant',
    name: 'AI Assistant'
  };

  messages: any[] = [
    {
      id: 1,
      timestamp: new Date(),
      author: this.aiUser,
      text: 'Hello! I can help you modify your jewelry design. Try saying things like "make it more modern" or "change the gemstone to ruby".'
    }
  ];

  onMessageEntered(e: any) {
    const userMessage = e.message;

    // Add user message to chat
    this.messages.push({
      id: this.messages.length + 1,
      timestamp: new Date(),
      author: this.currentUser,
      text: userMessage.text
    });

    // Emit to parent for processing
    this.onMessageSent.emit(userMessage.text);
  }

  addAiResponse(text: string) {
    this.messages.push({
      id: this.messages.length + 1,
      timestamp: new Date(),
      author: this.aiUser,
      text: text
    });
  }
}
```

#### 5.2.4 Image Gallery Component

**Purpose:** Display generated images with DxGallery

**Template (image-gallery.component.html):**

```html
<div class="image-gallery-container">
  <h3>Generated Images</h3>

  <dx-load-panel
    [visible]="loading"
    [showIndicator]="true"
    [showPane]="true"
    message="Generating image...">
  </dx-load-panel>

  <dx-gallery
    *ngIf="images.length > 0"
    [dataSource]="images"
    [height]="500"
    [loop]="true"
    [showNavButtons]="true"
    [showIndicator]="true"
    itemTemplate="galleryItem">

    <div *dxTemplate="let item of 'galleryItem'">
      <div class="gallery-item">
        <img [src]="item.fileServiceUrl" [alt]="item.fileName">
        <div class="image-info">
          <p>{{ item.generatedAt | date:'short' }}</p>
          <p>{{ item.generationPrompt }}</p>
          <dx-button
            text="Download"
            icon="download"
            (onClick)="downloadImage(item)">
          </dx-button>
        </div>
      </div>
    </div>
  </dx-gallery>

  <div *ngIf="images.length === 0 && !loading" class="empty-state">
    <p>No images generated yet</p>
    <p>Use the configuration form or chat to create your first jewelry design</p>
  </div>
</div>
```

### 5.3 Services Implementation

#### 5.3.1 ConstructorService

**Purpose:** HTTP API calls to Constructor microservice

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '@app/core/services/config.service';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConstructorService {
  private baseUrl: string;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    this.baseUrl = this.config.appSettings.HBGCONSTRUCTOR;
  }

  async getProjects(): Promise<any[]> {
    return firstValueFrom(
      this.http.get<any[]>(`${this.baseUrl}/api/projects`)
    );
  }

  async getProject(projectId: string): Promise<any> {
    return firstValueFrom(
      this.http.get<any>(`${this.baseUrl}/api/projects/${projectId}`)
    );
  }

  async createProject(project: any): Promise<any> {
    return firstValueFrom(
      this.http.post<any>(`${this.baseUrl}/api/projects`, project)
    );
  }

  async saveConfiguration(projectId: string, config: any): Promise<any> {
    return firstValueFrom(
      this.http.post<any>(
        `${this.baseUrl}/api/projects/${projectId}/configurations`,
        config
      )
    );
  }

  async generateImage(configurationId: string): Promise<any> {
    return firstValueFrom(
      this.http.post<any>(
        `${this.baseUrl}/api/configurations/${configurationId}/generate`,
        {}
      )
    );
  }

  async getProjectImages(projectId: string): Promise<any[]> {
    return firstValueFrom(
      this.http.get<any[]>(`${this.baseUrl}/api/projects/${projectId}/images`)
    );
  }
}
```

#### 5.3.2 ConstructorHubService

**Purpose:** SignalR real-time communication

```typescript
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { ConfigService } from '@app/core/services/config.service';
import { AuthService } from '@app/core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class ConstructorHubService {
  private hubConnection: HubConnection | null = null;
  private imageUpdateSubject = new BehaviorSubject<any>(null);

  public onImageUpdate$: Observable<any> = this.imageUpdateSubject.asObservable();

  constructor(
    private config: ConfigService,
    private auth: AuthService
  ) {}

  async connect(): Promise<void> {
    const token = this.auth.getAccessToken();
    const hubUrl = `${this.config.appSettings.HBGCONSTRUCTOR}/hubs/constructor`;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveImageUpdate', (data) => {
      this.imageUpdateSubject.next(data);
    });

    await this.hubConnection.start();
    console.log('Constructor Hub connected');
  }

  async sendChatMessage(projectId: string, message: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendChatMessage', projectId, message);
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
    }
  }
}
```

### 5.4 Routing Configuration

**File:** `src/app/modules/constructor/constructor.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { AuthGuard } from '@app/core/guards/auth.guard';
import { ConstructorMainComponent } from './components/constructor-main/constructor-main.component';
import { ProjectListComponent } from './components/project-list/project-list.component';

export const constructorRoutes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: ProjectListComponent
      },
      {
        path: ':id',
        component: ConstructorMainComponent
      }
    ]
  }
];
```

**Main app routes update** (`src/app/app.routes.ts`):

```typescript
{
  path: 'constructor',
  loadChildren: () => import('./modules/constructor/constructor.module')
    .then(m => m.ConstructorModule)
}
```

---

## 6. Kubernetes Deployment

### 6.1 Deployment Configuration

**File:** `k8s/hbg-constructor.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hbg-constructor
  namespace: default
spec:
  replicas: 2
  selector:
    matchLabels:
      app: hbg-constructor
  template:
    metadata:
      labels:
        app: hbg-constructor
    spec:
      containers:
      - name: hbg-constructor
        image: exdrums/hbg-constructor:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Development"
        - name: HBGCONSTRUCTORDB
          valueFrom:
            secretKeyRef:
              name: hbg-secret
              key: constructor_connection_string
        - name: HBGIDENTITY
          valueFrom:
            configMapKeyRef:
              name: hbg-configmap
              key: hbg-sts-url
        - name: HBGFILES
          value: "http://hbg-files-service"
        - name: HBGCONSTRUCTOR
          value: "http://hbg-constructor-service"
        - name: GEMINI_API_KEY
          valueFrom:
            secretKeyRef:
              name: hbg-secret
              key: gemini_api_key
        resources:
          requests:
            memory: "256Mi"
            cpu: "500m"
          limits:
            memory: "512Mi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: hbg-constructor-service
  namespace: default
spec:
  selector:
    app: hbg-constructor
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
```

### 6.2 Database Deployment

**File:** `k8s/hbg-constructor-db.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hbg-constructor-db
  namespace: default
spec:
  replicas: 1
  selector:
    matchLabels:
      app: hbg-constructor-db
  template:
    metadata:
      labels:
        app: hbg-constructor-db
    spec:
      containers:
      - name: postgres
        image: postgres:14
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_DB
          value: "hbgconstructordb"
        - name: POSTGRES_USER
          valueFrom:
            secretKeyRef:
              name: hbg-secret
              key: database_user
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: hbg-secret
              key: database_password
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: hbg-constructor-db-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: hbg-constructor-db-service
  namespace: default
spec:
  selector:
    app: hbg-constructor-db
  ports:
  - protocol: TCP
    port: 5432
    targetPort: 5432
  type: ClusterIP
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: hbg-constructor-db-pvc
  namespace: default
spec:
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 5Gi
```

### 6.3 Ingress Update

**File:** `k8s/hbg-ingress.yaml` (add this rule)

```yaml
- host: constructor.hbg.local
  http:
    paths:
    - path: /
      pathType: Prefix
      backend:
        service:
          name: hbg-constructor-service
          port:
            number: 80
```

### 6.4 ConfigMap & Secrets Update

**ConfigMap additions:**

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: hbg-configmap
data:
  # ... existing entries ...
  hbg-constructor-url: http://constructor.hbg.local
```

**Secret additions:**

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: hbg-secret
type: Opaque
data:
  # ... existing entries ...
  constructor_connection_string: <base64-encoded-connection-string>
  gemini_api_key: <base64-encoded-gemini-api-key>
```

---

## 7. Development Workflow

### 7.1 Phase 1: Backend Foundation (Week 1-2)

**Tasks:**
1. Create API.Constructor project structure
2. Set up ConstructorDbContext with all entities
3. Create initial EF Core migration
4. Implement basic Controllers (Projects, Configurations)
5. Add authentication middleware
6. Set up Swagger/OpenAPI documentation
7. Create unit tests for services

**Deliverables:**
- Working API with CRUD operations
- Database schema created
- Authentication working with Identity Server

### 7.2 Phase 2: AI Integration (Week 2-3)

**Tasks:**
1. Install Google_GenerativeAI NuGet package
2. Implement GeminiImageService
3. Implement ConfigurationService (prompt building)
4. Test image generation with various configurations
5. Add error handling and retry logic
6. Implement rate limiting for API calls

**Deliverables:**
- Working AI image generation
- Prompt templates for all jewelry types
- Service resilience patterns

### 7.3 Phase 3: Files Service Integration (Week 3)

**Tasks:**
1. Create ImagesController in Files service
2. Implement image storage service
3. Add thumbnail generation
4. Update FilesServiceClient in Constructor
5. Test end-to-end image flow

**Deliverables:**
- Images stored in Files service
- Accessible via URLs
- Thumbnail generation working

### 7.4 Phase 4: SignalR & Real-time (Week 4)

**Tasks:**
1. Create ConstructorHub
2. Implement chat message processing
3. Add real-time image update notifications
4. Test concurrent users
5. Add connection management

**Deliverables:**
- Real-time updates working
- Chat integration functional
- Multi-user support

### 7.5 Phase 5: Frontend Implementation (Week 5-6)

**Tasks:**
1. Create Constructor Angular module
2. Implement all components (Form, Chat, Gallery)
3. Set up routing
4. Implement ConstructorService
5. Implement ConstructorHubService
6. Add state management
7. Style with DevExtreme themes

**Deliverables:**
- Complete Angular module
- All UI components working
- Real-time updates in UI

### 7.6 Phase 6: Kubernetes Deployment (Week 7)

**Tasks:**
1. Create Dockerfile for Constructor API
2. Build and push Docker image
3. Create Kubernetes manifests
4. Deploy to cluster
5. Update Ingress
6. Configure secrets
7. Test in cluster environment

**Deliverables:**
- Constructor running in k8s
- Accessible via Ingress
- All services communicating

### 7.7 Phase 7: Testing & Optimization (Week 8)

**Tasks:**
1. End-to-end testing
2. Performance optimization
3. Load testing
4. Security audit
5. Documentation
6. Bug fixes

**Deliverables:**
- Production-ready application
- Complete documentation
- Test coverage > 80%

---

## 8. Technical Considerations

### 8.1 Google Gemini API

**Recommended Library:** `Google_GenerativeAI` (v3.4.0+)

**Reasons:**
1. Most complete C# SDK for Google Generative AI
2. Native support for Imagen-3.0
3. Active development and maintenance
4. Easy-to-use API
5. Good documentation

**Alternative:** Official Google Gen AI .NET SDK (newer but less features)

### 8.2 DevExtreme Best Practices

**For Angular Integration (2025):**

1. **Use Standalone Components Pattern**
   - Angular 19+ default
   - Import only required components
   - No NgModules needed

2. **Use New Configuration Components**
   - Strictly typed
   - Better IntelliSense
   - Future-proof

3. **DxChat Integration**
   - Available in v24.2+
   - Built-in AI assistant support
   - Easy message handling

4. **DxForm Dynamic Configuration**
   - Use `formData` binding
   - Leverage validation rules
   - Template-driven forms

### 8.3 Performance Optimization

**Image Generation:**
- Implement queue system for multiple requests
- Cache generated images
- Use CDN for image delivery
- Implement image lazy loading

**Database:**
- Index frequently queried columns
- Use pagination for large datasets
- Implement caching (Redis)
- Database connection pooling

**Frontend:**
- Lazy load Constructor module
- Use Angular OnPush change detection
- Implement virtual scrolling for galleries
- Optimize bundle size

### 8.4 Security Considerations

**API Security:**
- All endpoints require authentication
- Implement rate limiting
- Validate all inputs
- Sanitize prompts before sending to AI
- Implement CORS properly

**Data Security:**
- Encrypt sensitive data at rest
- Use HTTPS everywhere
- Store API keys in secrets
- Implement user data isolation
- Audit logging for all operations

### 8.5 Error Handling

**AI Service Errors:**
- Retry with exponential backoff
- Fallback to default configurations
- User-friendly error messages
- Log all failures

**Network Errors:**
- Handle timeout scenarios
- Implement circuit breaker pattern
- Show loading states
- Offline support (PWA)

---

## 9. Estimated Costs

### 9.1 Google Gemini API

**Imagen-3.0 Pricing:**
- $0.03 per image generated
- Estimated usage: 1000 images/month
- **Monthly cost: ~$30**

### 9.2 Infrastructure

**Kubernetes Resources:**
- Constructor API: 2 replicas × 512MB = 1GB RAM
- Database: 1 replica × 2GB = 2GB RAM
- Storage: 5GB for database + 20GB for images = 25GB

**Estimated cloud cost (if using managed k8s):**
- **~$50-100/month** (depending on provider)

### 9.3 Total Estimated Cost

**Development:** 8 weeks × 40 hours = 320 hours
**Monthly Operational:** ~$80-130

---

## 10. Success Criteria

1. ✅ Users can create jewelry projects
2. ✅ DxForm generates accurate AI prompts
3. ✅ Images generate within 10 seconds
4. ✅ Chat modifications update images in real-time
5. ✅ All images stored and retrievable
6. ✅ Multi-user support with proper isolation
7. ✅ Mobile-responsive UI
8. ✅ 99% uptime in production
9. ✅ Secure authentication/authorization
10. ✅ Comprehensive documentation

---

## 11. Future Enhancements

**Phase 2 Features:**
- Image editing and variations
- Template library
- Collaborative projects
- Export to 3D models
- AR/VR preview
- Social sharing
- Marketplace integration
- Advanced AI training on custom designs

---

## Appendix A: API Endpoints

### Projects
- `GET /api/projects` - List user projects
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Configurations
- `POST /api/projects/{id}/configurations` - Save configuration
- `GET /api/configurations/{id}` - Get configuration
- `POST /api/configurations/{id}/generate` - Generate image

### Images
- `GET /api/projects/{id}/images` - List project images
- `GET /api/images/{id}` - Get image details
- `DELETE /api/images/{id}` - Delete image
- `GET /api/images/{id}/download` - Download image

### Chat (SignalR Hub)
- `SendChatMessage(projectId, message)` - Send chat message
- `RegenerateImage(configurationId)` - Regenerate image
- `ReceiveImageUpdate(data)` - Client receives update

---

## Appendix B: Database Indexes

```sql
-- Performance indexes
CREATE INDEX IX_ConstructorProjects_UserId_CreatedAt
  ON ConstructorProjects(UserId, CreatedAt DESC);

CREATE INDEX IX_ProjectConfigurations_ProjectId_UpdatedAt
  ON ProjectConfigurations(ProjectId, UpdatedAt DESC);

CREATE INDEX IX_GeneratedImages_ConfigurationId_GeneratedAt
  ON GeneratedImages(ConfigurationId, GeneratedAt DESC);

CREATE INDEX IX_ChatInteractions_ProjectId_CreatedAt
  ON ChatInteractions(ProjectId, CreatedAt DESC);

-- Full-text search (if needed)
CREATE INDEX IX_ConstructorProjects_Name_GIN
  ON ConstructorProjects USING gin(to_tsvector('english', Name));
```

---

## Appendix C: Example Prompts

**Ring - Classic Gold with Diamond:**
```
A hyperrealistic 3D render of a ring, made of gold, featuring diamond gemstone,
classic style, polished finish, high-quality product photography, studio lighting,
white background, 4K, detailed craftsmanship
```

**Earring - Modern Silver:**
```
A hyperrealistic 3D render of a earring, made of silver, modern style, brushed finish,
high-quality product photography, studio lighting, white background, 4K,
detailed craftsmanship
```

**Necklace - Vintage with Ruby:**
```
A hyperrealistic 3D render of a necklace, made of rose gold, featuring ruby gemstone,
vintage style, antiqued finish, high-quality product photography, studio lighting,
white background, 4K, detailed craftsmanship
```

---

**Document Version:** 1.0
**Last Updated:** 2025-11-11
**Author:** AI Development Agent
**Status:** Ready for Implementation
