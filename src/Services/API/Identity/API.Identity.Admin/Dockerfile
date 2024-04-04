FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API.Identity.Admin/API.Identity.Admin.csproj", "API.Identity.Admin/"]
COPY ["API.Identity.Admin.EntityFramework.Shared/API.Identity.Admin.EntityFramework.Shared.csproj", "API.Identity.Admin.EntityFramework.Shared/"]
COPY ["API.Identity.Admin.EntityFramework.SqlServer/API.Identity.Admin.EntityFramework.SqlServer.csproj", "API.Identity.Admin.EntityFramework.SqlServer/"]
COPY ["API.Identity.Admin.EntityFramework.PostgreSQL/API.Identity.Admin.EntityFramework.PostgreSQL.csproj", "API.Identity.Admin.EntityFramework.PostgreSQL/"]
COPY ["API.Identity.Shared/API.Identity.Shared.csproj", "API.Identity.Shared/"]
COPY ["API.Identity.Admin.EntityFramework.MySql/API.Identity.Admin.EntityFramework.MySql.csproj", "API.Identity.Admin.EntityFramework.MySql/"]
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