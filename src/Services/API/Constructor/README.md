# API.Constructor - Jewelry Constructor Microservice

## Overview

The Jewelry Constructor microservice provides AI-powered jewelry design and image generation capabilities using Google's Gemini Imagen-3.0 model.

## Features

- **Project Management**: Create and manage jewelry design projects
- **Form-based Configuration**: Define jewelry parameters (type, material, gemstone, style, finish)
- **AI Image Generation**: Generate realistic jewelry images from text prompts
- **Real-time Communication**: SignalR hub for chat-based modifications
- **Image Storage**: Integration with Files service for image persistence

## Technologies

- ASP.NET Core 9.0
- Entity Framework Core 9.0
- PostgreSQL
- Google Gemini AI (Google_GenerativeAI SDK)
- SignalR
- AutoMapper
- Swagger/OpenAPI

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL database
- Google Gemini API Key
- Identity Server running (for authentication)
- Files service running (for image storage)

### Configuration

Update `appsettings.json` with your settings:

```json
{
  "HBGCONSTRUCTORDB": "Server=localhost;port=32345;Database=hbgconstructordb;Uid=hbg-dbuser;Pwd=your-password",
  "HBGIDENTITY": "https://localhost:5700",
  "HBGFILES": "http://localhost:5701",
  "AUDIENCE": "api_constructor",
  "GeminiSettings": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "imagen-3.0-generate-002"
  }
}
```

### Run Locally

```bash
cd src/Services/API/Constructor/API.Constructor
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5705`

### Run with Docker

```bash
docker build -t hbg-constructor:latest -f src/Services/API/Constructor/API.Constructor/Dockerfile .
docker run -p 5705:80 hbg-constructor:latest
```

## API Endpoints

### Projects
- `GET /api/projects` - List user projects
- `POST /api/projects` - Create new project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/projects/{id}/images` - Get project images

### Configurations
- `GET /api/configurations/{id}` - Get configuration
- `POST /api/configurations` - Save configuration
- `POST /api/configurations/{id}/generate` - Generate image

### Images
- `GET /api/images/{id}` - Get image details
- `DELETE /api/images/{id}` - Delete image

### SignalR Hub
- **Endpoint**: `/hubs/constructor`
- **Methods**:
  - `SendChatMessage(projectId, message)` - Send chat message
  - `RegenerateImage(configurationId)` - Regenerate image

## Database Schema

The service uses PostgreSQL with the following tables:
- `ConstructorProjects` - User projects
- `ProjectConfigurations` - Jewelry configurations
- `GeneratedImages` - Generated images metadata
- `ChatInteractions` - Chat history

Migrations are automatically applied on startup.

## Authentication

All endpoints require JWT Bearer authentication from the Identity Server.

## Development

### Add Migration

```bash
dotnet ef migrations add MigrationName
```

### Update Database

```bash
dotnet ef database update
```

## License

Copyright © 2025
