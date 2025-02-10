FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API.Identity.Shared/API.Identity.Shared.csproj", "API.Identity.Shared/"]
COPY ["API.Identity.Shared.Configuration/API.Identity.Shared.Configuration.csproj", "API.Identity.Shared.Configuration/"]
COPY ["API.Identity/API.Identity.csproj", "API.Identity/"]
COPY ["API.Identity.Admin.EntityFramework/API.Identity.Admin.EntityFramework.csproj", "API.Identity.Admin.EntityFramework/"]
COPY ["API.Identity.Admin.EntityFramework.Shared/API.Identity.Admin.EntityFramework.Shared.csproj", "API.Identity.Admin.EntityFramework.Shared/"]
COPY ["API.Identity.Admin.EntityFramework.Configuration/API.Identity.Admin.EntityFramework.Configuration.csproj", "API.Identity.Admin.EntityFramework.Configuration/"]
COPY ["API.Identity.Admin.EntityFramework.Identity/API.Identity.Admin.EntityFramework.Identity.csproj", "API.Identity.Admin.EntityFramework.Identity/"]
COPY ["API.Identity.Admin.EntityFramework.Extensions/API.Identity.Admin.EntityFramework.Extensions.csproj", "API.Identity.Admin.EntityFramework.Extensions/"]
COPY ["API.Identity.Admin.BusinessLogic.Identity/API.Identity.Admin.BusinessLogic.Identity.csproj", "API.Identity.Admin.BusinessLogic.Identity/"]
COPY ["API.Identity.Admin.BusinessLogic.Shared/API.Identity.Admin.BusinessLogic.Shared.csproj", "API.Identity.Admin.BusinessLogic.Shared/"]
COPY ["API.Identity.AuditLogging/API.Identity.AuditLogging.csproj", "API.Identity.AuditLogging/"]
COPY ["API.Identity.AuditLogging.EntityFramework/API.Identity.AuditLogging.EntityFramework.csproj", "API.Identity.AuditLogging.EntityFramework/"]

RUN dotnet restore "API.Identity/API.Identity.csproj"
COPY . .
WORKDIR "/src/API.Identity"
RUN dotnet build "API.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "API.Identity.dll"]