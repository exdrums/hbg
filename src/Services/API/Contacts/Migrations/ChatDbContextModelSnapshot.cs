﻿// <auto-generated />
using System;
using API.Contacts.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Contacts.Migrations
{
    [DbContext(typeof(ChatDbContext))]
    partial class ChatDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("API.Contacts.Models.Conversation", b =>
                {
                    b.Property<Guid>("ConversationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedByUserId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastMessageAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastMessagePreview")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("ConversationId");

                    b.HasIndex("Type");

                    b.HasIndex("IsActive", "LastMessageAt");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("API.Contacts.Models.ConversationParticipant", b =>
                {
                    b.Property<Guid>("ConversationId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastReadAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LeftAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NotificationPreference")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasDefaultValue("All");

                    b.Property<string>("Role")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasDefaultValue("Member");

                    b.Property<int>("UnreadCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.HasKey("ConversationId", "UserId");

                    b.HasIndex("UserId");

                    b.HasIndex("ConversationId", "UserId")
                        .IsUnique();

                    b.ToTable("ConversationParticipants");
                });

            modelBuilder.Entity("API.Contacts.Models.Message", b =>
                {
                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ConversationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("EditedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Metadata")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ReplyToMessageId")
                        .HasColumnType("uuid");

                    b.Property<string>("SenderUserId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("MessageId");

                    b.HasIndex("ReplyToMessageId");

                    b.HasIndex("SenderUserId");

                    b.HasIndex("Type");

                    b.HasIndex("ConversationId", "SentAt");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("API.Contacts.Models.MessageReadReceipt", b =>
                {
                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("ReadAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("MessageId", "UserId");

                    b.HasIndex("MessageId", "UserId")
                        .IsUnique();

                    b.HasIndex("UserId", "ReadAt");

                    b.ToTable("MessageReadReceipts");
                });

            modelBuilder.Entity("API.Contacts.Models.ConversationParticipant", b =>
                {
                    b.HasOne("API.Contacts.Models.Conversation", "Conversation")
                        .WithMany("Participants")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Conversation");
                });

            modelBuilder.Entity("API.Contacts.Models.Message", b =>
                {
                    b.HasOne("API.Contacts.Models.Conversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("API.Contacts.Models.Message", "ReplyToMessage")
                        .WithMany()
                        .HasForeignKey("ReplyToMessageId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Conversation");

                    b.Navigation("ReplyToMessage");
                });

            modelBuilder.Entity("API.Contacts.Models.MessageReadReceipt", b =>
                {
                    b.HasOne("API.Contacts.Models.Message", "Message")
                        .WithMany("ReadReceipts")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("API.Contacts.Models.Conversation", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("Participants");
                });

            modelBuilder.Entity("API.Contacts.Models.Message", b =>
                {
                    b.Navigation("ReadReceipts");
                });
#pragma warning restore 612, 618
        }
    }
}
