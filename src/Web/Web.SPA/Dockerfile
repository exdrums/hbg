# Created to build from VS Code Workspace Root (see workspace file)

ARG NODE_IMAGE=node:20-trixie
# ARG NODE_IMAGE=node:trixie

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR "/src"
COPY ["src/Web/Web.SPA/Web.SPA.csproj", "Web/Web.SPA/"]
COPY ["src/Common/Common/Common.csproj", "Common/Common/"]
RUN dotnet restore "Web/Web.SPA/Web.SPA.csproj"

COPY ["src/Web/Web.SPA/.", "Web/Web.SPA/"]
COPY ["src/Common/Common/.", "Common/Common/"]
WORKDIR "/src/Web/Web.SPA/."
RUN dotnet build "Web.SPA.csproj" -c Release -o /app

FROM ${NODE_IMAGE} as node-build
WORKDIR /src
COPY ["src/Web/Web.SPA/Client/", "."]
RUN npm ci
RUN npm run build

FROM build AS publish
RUN dotnet publish "Web.SPA.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=node-build /src/dist ./Client/dist
ENTRYPOINT ["dotnet", "Web.SPA.dll"]