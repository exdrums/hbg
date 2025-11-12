FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/Services/API/Constructor/API.Constructor/API.Constructor.csproj", "src/Services/API/Constructor/API.Constructor/"]
COPY ["src/Common/Common/Common.csproj", "src/Common/Common/"]
RUN dotnet restore "src/Services/API/Constructor/API.Constructor/API.Constructor.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Services/API/Constructor/API.Constructor"
RUN dotnet build "API.Constructor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Constructor.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Constructor.dll"]
