﻿// <auto-generated />
using System;
using API.Identity.Admin.EntityFramework.Shared.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Identity.Admin.EntityFramework.PostgreSQL.Migrations.Logging
{
    [DbContext(typeof(AdminLogDbContext))]
    [Migration("20221016191311_HbgInit")]
    partial class HbgInit
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("API.Identity.Admin.EntityFramework.Entities.Log", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Exception")
                        .HasColumnType("text");

                    b.Property<string>("Level")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("LogEvent")
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<string>("MessageTemplate")
                        .HasColumnType("text");

                    b.Property<string>("Properties")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Log", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
