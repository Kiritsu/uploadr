﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UploadR.Database;

namespace UploadR.Database.Migrations
{
    [DbContext(typeof(UploadRContext))]
    [Migration("20200507051446_unverified_user")]
    partial class unverified_user
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("UploadR.Database.Models.ShortenedUrl", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("guid")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthorGuid")
                        .HasColumnName("author_guid")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValue(new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(3801));

                    b.Property<TimeSpan>("ExpiryTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("expiry_time")
                        .HasColumnType("interval")
                        .HasDefaultValue(new TimeSpan(0, 0, 0, 0, 0));

                    b.Property<DateTime>("LastSeen")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("last_seen")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValue(new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(4116));

                    b.Property<string>("Password")
                        .HasColumnName("password_hash")
                        .HasColumnType("text");

                    b.Property<bool>("Removed")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("removed")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<long>("SeenCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("seen_count")
                        .HasColumnType("bigint")
                        .HasDefaultValue(0L);

                    b.Property<string>("Shorten")
                        .IsRequired()
                        .HasColumnName("shorten")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnName("url")
                        .HasColumnType("text");

                    b.HasKey("Guid")
                        .HasName("pk_shortenedurl_guid");

                    b.HasIndex("AuthorGuid");

                    b.ToTable("shortenedurls");
                });

            modelBuilder.Entity("UploadR.Database.Models.Upload", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("guid")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthorGuid")
                        .HasColumnName("author_guid")
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnName("content_type")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValue(new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8380));

                    b.Property<TimeSpan>("ExpiryTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("expiry_time")
                        .HasColumnType("interval")
                        .HasDefaultValue(new TimeSpan(0, 0, 0, 0, 0));

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnName("file_name")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastSeen")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("last_seen")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValue(new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8787));

                    b.Property<string>("Password")
                        .HasColumnName("password_hash")
                        .HasColumnType("text");

                    b.Property<bool>("Removed")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("removed")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<long>("SeenCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("seen_count")
                        .HasColumnType("bigint")
                        .HasDefaultValue(0L);

                    b.HasKey("Guid")
                        .HasName("pk_upload_guid");

                    b.HasIndex("AuthorGuid");

                    b.ToTable("uploads");
                });

            modelBuilder.Entity("UploadR.Database.Models.User", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("guid")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValue(new DateTime(2020, 5, 7, 7, 14, 46, 734, DateTimeKind.Local).AddTicks(8877));

                    b.Property<bool>("Disabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("disabled")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnName("api_token")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("account_type")
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.HasKey("Guid")
                        .HasName("pk_user_guid");

                    b.ToTable("users");
                });

            modelBuilder.Entity("UploadR.Database.Models.ShortenedUrl", b =>
                {
                    b.HasOne("UploadR.Database.Models.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UploadR.Database.Models.Upload", b =>
                {
                    b.HasOne("UploadR.Database.Models.User", "Author")
                        .WithMany("Uploads")
                        .HasForeignKey("AuthorGuid")
                        .HasConstraintName("fkey_user_authorid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
