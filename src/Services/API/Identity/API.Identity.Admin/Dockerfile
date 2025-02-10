FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API.Identity.Shared/API.Identity.Shared.csproj", "API.Identity.Shared/"]
COPY ["API.Identity.Shared.Configuration/API.Identity.Shared.Configuration.csproj", "API.Identity.Shared.Configuration/"]
COPY ["API.Identity.Admin/API.Identity.Admin.csproj", "API.Identity.Admin/"]
COPY ["API.Identity.Admin.EntityFramework/API.Identity.Admin.EntityFramework.csproj", "API.Identity.Admin.EntityFramework/"]
COPY ["API.Identity.Admin.EntityFramework.Shared/API.Identity.Admin.EntityFramework.Shared.csproj", "API.Identity.Admin.EntityFramework.Shared/"]
COPY ["API.Identity.Admin.EntityFramework.Configuration/API.Identity.Admin.EntityFramework.Configuration.csproj", "API.Identity.Admin.EntityFramework.Configuration/"]
COPY ["API.Identity.Admin.EntityFramework.SqlServer/API.Identity.Admin.EntityFramework.SqlServer.csproj", "API.Identity.Admin.EntityFramework.SqlServer/"]
COPY ["API.Identity.Admin.EntityFramework.PostgreSQL/API.Identity.Admin.EntityFramework.PostgreSQL.csproj", "API.Identity.Admin.EntityFramework.PostgreSQL/"]
COPY ["API.Identity.Admin.EntityFramework.MySql/API.Identity.Admin.EntityFramework.MySql.csproj", "API.Identity.Admin.EntityFramework.MySql/"]
COPY ["API.Identity.Admin.EntityFramework.Identity/API.Identity.Admin.EntityFramework.Identity.csproj", "API.Identity.Admin.EntityFramework.Identity/"]
COPY ["API.Identity.Admin.EntityFramework.Extensions/API.Identity.Admin.EntityFramework.Extensions.csproj", "API.Identity.Admin.EntityFramework.Extensions/"]
COPY ["API.Identity.Admin.UI/API.Identity.Admin.UI.csproj", "API.Identity.Admin.UI/"]
COPY ["API.Identity.Admin.BusinessLogic.Identity/API.Identity.Admin.BusinessLogic.Identity.csproj", "API.Identity.Admin.BusinessLogic.Identity/"]
COPY ["API.Identity.Admin.BusinessLogic/API.Identity.Admin.BusinessLogic.csproj", "API.Identity.Admin.BusinessLogic/"]
COPY ["API.Identity.Admin.BusinessLogic.Shared/API.Identity.Admin.BusinessLogic.Shared.csproj", "API.Identity.Admin.BusinessLogic.Shared/"]
COPY ["API.Identity.AuditLogging.EntityFramework/API.Identity.AuditLogging.EntityFramework.csproj", "API.Identity.AuditLogging.EntityFramework/"]
COPY ["API.Identity.AuditLogging/API.Identity.AuditLogging.csproj", "API.Identity.AuditLogging/"]
RUN dotnet restore "API.Identity.Admin/API.Identity.Admin.csproj"
COPY . .
WORKDIR "/src/API.Identity.Admin"
RUN dotnet build "API.Identity.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Identity.Admin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "API.Identity.Admin.dll"]