# Created to build from VS Code Workspace Root (see workspace file)

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/API/Projects/API.Projects.csproj", "Services/API/Projects/"]
COPY ["src/Common/Common/Common.csproj", "Common/Common/"]
RUN dotnet restore "Services/API/Projects/API.Projects.csproj"

COPY ["src/Services/API/Projects/.", "Services/API/Projects/"]
COPY ["src/Common/Common/.", "Common/Common/"]
WORKDIR "/src/Services/API/Projects"
RUN dotnet build "API.Projects.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "API.Projects.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "API.Projects.dll"]