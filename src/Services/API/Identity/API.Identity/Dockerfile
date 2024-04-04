FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API.Identity/API.Identity.csproj", "API.Identity/"]
COPY ["API.Identity.Admin.EntityFramework.Shared/API.Identity.Admin.EntityFramework.Shared.csproj", "API.Identity.Admin.EntityFramework.Shared/"]
COPY ["API.Identity.Shared/API.Identity.Shared.csproj", "API.Identity.Shared/"]
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