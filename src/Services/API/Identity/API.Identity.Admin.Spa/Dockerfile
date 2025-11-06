# Multi-stage Dockerfile for Identity Admin SPA
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/Services/API/Identity/API.Identity.Admin.Spa/API.Identity.Admin.Spa.csproj", "Services/API/Identity/API.Identity.Admin.Spa/"]
COPY ["src/Common/Common/Common.csproj", "Common/Common/"]

RUN dotnet restore "Services/API/Identity/API.Identity.Admin.Spa/API.Identity.Admin.Spa.csproj"

# Copy all source files
COPY src/Services/API/Identity/API.Identity.Admin.Spa/ Services/API/Identity/API.Identity.Admin.Spa/
COPY src/Common/ Common/

# Build .NET project
WORKDIR "/src/Services/API/Identity/API.Identity.Admin.Spa"
RUN dotnet build "API.Identity.Admin.Spa.csproj" -c Release -o /app/build

# Stage 3: Build Angular application
FROM node:18-alpine AS angular-build
WORKDIR /src

# Copy package files
COPY src/Services/API/Identity/API.Identity.Admin.Spa/Client/package*.json ./

# Install dependencies
RUN npm ci --production=false

# Copy Angular source
COPY src/Services/API/Identity/API.Identity.Admin.Spa/Client/ ./

# Build Angular app
RUN npm run build

# Stage 4: Publish .NET application
FROM build AS publish
RUN dotnet publish "API.Identity.Admin.Spa.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 5: Final runtime image
FROM base AS final
WORKDIR /app

# Copy published .NET app
COPY --from=publish /app/publish .

# Copy built Angular app
COPY --from=angular-build /src/dist ./Client/dist

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "API.Identity.Admin.Spa.dll"]
