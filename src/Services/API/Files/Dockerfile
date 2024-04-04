# Created to build from VS Code Workspace Root (see workspace file)

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/API/Files/API.Files.csproj", "Services/API/Files/"]
COPY ["src/Common/Common/Common.csproj", "Common/Common/"]
RUN dotnet restore "Services/API/Files/API.Files.csproj"

COPY ["src/Services/API/Files/.", "Services/API/Files/"]
COPY ["src/Common/Common/.", "Common/Common/"]
WORKDIR "/src/Services/API/Files"
RUN dotnet build "API.Files.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "API.Files.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "API.Files.dll"]