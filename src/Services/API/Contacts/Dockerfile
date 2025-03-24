# Created to build from VS Code Workspace Root (see workspace file)

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/API/Contacts/API.Contacts.csproj", "Services/API/Contacts/"]
COPY ["src/Common/Common/Common.csproj", "Common/Common/"]
RUN dotnet restore "Services/API/Contacts/API.Contacts.csproj"

COPY ["src/Services/API/Contacts/.", "Services/API/Contacts/"]
COPY ["src/Common/Common/.", "Common/Common/"]
WORKDIR "/src/Services/API/Contacts"
RUN dotnet build "API.Contacts.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "API.Contacts.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "API.Contacts.dll"] 