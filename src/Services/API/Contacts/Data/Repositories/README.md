# Database Migrations for Chat Microservice

This document describes how to work with Entity Framework Core migrations for the Chat Microservice.

## Prerequisites

- .NET SDK 6.0 or later
- PostgreSQL database server
- Entity Framework Core CLI tools

## Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

## Adding a New Migration

When you make changes to the domain model or entity configurations, you need to create a new migration:

```bash
dotnet ef migrations add MigrationName --project ChatService.Infrastructure --startup-project ChatService.API
```

## Applying Migrations to the Database

To update the database with the latest migrations:

```bash
dotnet ef database update --project ChatService.Infrastructure --startup-project ChatService.API
```

## Removing the Last Migration

If you need to remove the last migration (only if it hasn't been applied to the database):

```bash
dotnet ef migrations remove --project ChatService.Infrastructure --startup-project ChatService.API
```

## Generate SQL Script

To generate a SQL script for a migration (useful for production deployments):

```bash
dotnet ef migrations script PreviousMigration CurrentMigration --output migration.sql --project ChatService.Infrastructure --startup-project ChatService.API
```

## Database Schema Design

The database schema follows the domain model with the following key tables:

1. `users` - Stores user information and preferences
2. `conversations` - Stores chat conversations
3. `conversation_participants` - Stores the relationship between users and conversations
4. `messages` - Stores all chat messages
5. `alerts` - Stores system alerts and notifications

## PostgreSQL-Specific Features

- The application uses `jsonb` for storing user preferences, which allows for efficient querying of JSON data
- Timestamp fields use `timestamp with time zone` to ensure proper timezone handling
- Indexes are created for all frequent query patterns to ensure performance

## Connection String

Make sure to set up your PostgreSQL connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatservice;Username=postgres;Password=your_password"
  }
}
```