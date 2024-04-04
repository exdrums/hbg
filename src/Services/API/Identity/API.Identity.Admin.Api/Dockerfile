FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/API.Identity.Admin.Api/API.Identity.Admin.Api.csproj", "src/API.Identity.Admin.Api/"]
COPY ["src/API.Identity.Admin.EntityFramework.Shared/API.Identity.Admin.EntityFramework.Shared.csproj", "src/API.Identity.Admin.EntityFramework.Shared/"]
COPY ["src/API.Identity.Admin.EntityFramework.SqlServer/API.Identity.Admin.EntityFramework.SqlServer.csproj", "src/API.Identity.Admin.EntityFramework.SqlServer/"]
COPY ["src/API.Identity.Admin.EntityFramework.PostgreSQL/API.Identity.Admin.EntityFramework.PostgreSQL.csproj", "src/API.Identity.Admin.EntityFramework.PostgreSQL/"]
COPY ["src/API.Identity.Shared/API.Identity.Shared.csproj", "src/API.Identity.Shared/"]
COPY ["src/API.Identity.Admin.EntityFramework.MySql/API.Identity.Admin.EntityFramework.MySql.csproj", "src/API.Identity.Admin.EntityFramework.MySql/"]
RUN dotnet restore "src/API.Identity.Admin.Api/API.Identity.Admin.Api.csproj"
COPY . .
WORKDIR "/src/src/API.Identity.Admin.Api"
RUN dotnet build "API.Identity.Admin.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Identity.Admin.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "API.Identity.Admin.Api.dll"]