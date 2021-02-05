using System;
using Microsoft.EntityFrameworkCore;
using UploadR.Database.Enums;
using UploadR.Database.Models;

namespace UploadR.Database
{
    public class UploadRContext : DbContext
    {
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        public UploadRContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region User
            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<User>()
                .Property(x => x.Guid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("guid");

            modelBuilder.Entity<User>()
                .HasKey(x => x.Guid)
                .HasName("pk_user_guid");

            modelBuilder.Entity<User>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasColumnName("email");

            modelBuilder.Entity<User>()
                .Property(x => x.Disabled)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("disabled");

            modelBuilder.Entity<User>()
                .Property(x => x.Type)
                .IsRequired()
                .HasDefaultValue(AccountType.Unverified)
                .HasColumnName("account_type");

            modelBuilder.Entity<User>()
                .Property(x => x.Token)
                .IsRequired()
                .HasColumnName("api_token");

            modelBuilder.Entity<User>()
                .HasMany(x => x.Uploads)
                .WithOne(x => x.Author)
                .HasForeignKey(x => x.AuthorGuid)
                .HasConstraintName("fkey_user_authorid")
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<User>()
                .HasMany(x => x.Shortens)
                .WithOne(x => x.Author)
                .HasForeignKey(x => x.AuthorGuid)
                .HasConstraintName("fkey_user_authorid")
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Upload
            modelBuilder.Entity<Upload>()
                .ToTable("uploads");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Guid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("guid");

            modelBuilder.Entity<Upload>()
                .HasKey(x => x.Guid)
                .HasName("pk_upload_guid");

            modelBuilder.Entity<Upload>()
                .Property(x => x.AuthorGuid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("author_guid");

            modelBuilder.Entity<Upload>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<Upload>()
                .Property(x => x.LastSeen)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("last_seen");

            modelBuilder.Entity<Upload>()
                .Property(x => x.ExpiryTime)
                .IsRequired()
                .HasDefaultValue(TimeSpan.Zero)
                .HasColumnName("expiry_time");

            modelBuilder.Entity<Upload>()
                .Property(x => x.SeenCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("seen_count");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Removed)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("removed");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Identifier)
                .IsRequired()
                .HasColumnName("file_name");

            modelBuilder.Entity<Upload>()
                .Property(x => x.ContentType)
                .IsRequired()
                .HasColumnName("content_type");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Password)
                .HasColumnName("password_hash");
            #endregion
            
            #region ShortenedUrls
            modelBuilder.Entity<ShortenedUrl>()
                .ToTable("shortenedurls");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.Guid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("guid");

            modelBuilder.Entity<ShortenedUrl>()
                .HasKey(x => x.Guid)
                .HasName("pk_shortenedurl_guid");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.AuthorGuid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("author_guid");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.LastSeen)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("last_seen");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.ExpiryTime)
                .IsRequired()
                .HasDefaultValue(TimeSpan.Zero)
                .HasColumnName("expiry_time");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.SeenCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("seen_count");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.Removed)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("removed");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.Url)
                .IsRequired()
                .HasColumnName("url");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.Identifier)
                .IsRequired()
                .HasColumnName("shorten");

            modelBuilder.Entity<ShortenedUrl>()
                .Property(x => x.Password)
                .HasColumnName("password_hash");
            #endregion
        }
    }
}
