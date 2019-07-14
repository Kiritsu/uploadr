using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShareY.Database.Models;

namespace ShareY.Database
{
    public class ShareYContext : DbContext
    {
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string _connectionString = "Host=localhost;Database=sharey;Username=sharey;Password=1234";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region User
            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<User>()
                .Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id)
                .HasName("pk_user_id");

            modelBuilder.Entity<User>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .IsRequired()
                .HasColumnName("email");

            modelBuilder.Entity<User>()
                .HasAlternateKey(x => x.Email)
                .HasName("ak_user_email");

            modelBuilder.Entity<User>()
                .Property(x => x.Disabled)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("disabled");
            #endregion

            #region Token
            modelBuilder.Entity<Token>()
                .ToTable("tokens");

            modelBuilder.Entity<Token>()
                .Property(x => x.Guid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("guid");

            modelBuilder.Entity<Token>()
                .HasKey(x => x.Guid)
                .HasName("key_token_guid");

            modelBuilder.Entity<Token>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<Token>()
                .Property(x => x.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            modelBuilder.Entity<Token>()
                .HasIndex(x => x.UserId)
                .IsUnique()
                .HasName("index_user_id");

            modelBuilder.Entity<Token>()
                .HasOne(x => x.User)
                .WithOne(x => x.Token)
                .HasForeignKey("fuck if i know what to put there")
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_token_userid");
            #endregion

            #region Upload
            modelBuilder.Entity<Upload>()
                .ToTable("uploads");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            modelBuilder.Entity<Upload>()
                .HasKey(x => x.Id)
                .HasName("key_upload_id");

            modelBuilder.Entity<Upload>()
                .Property(x => x.AuthorId)
                .IsRequired()
                .HasColumnName("author_id");

            modelBuilder.Entity<Upload>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<Upload>()
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("download_count");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Visible)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("visible");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Removed)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("removed");

            modelBuilder.Entity<Upload>()
                .Property(x => x.UploadType)
                .IsRequired()
                .HasColumnName("upload_type");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Content)
                .IsRequired()
                .HasColumnName("content");

            modelBuilder.Entity<Upload>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Uploads)
                .HasForeignKey(x => x.AuthorId)
                .HasConstraintName("fkey_upload_authorid");
            #endregion
        }
    }
}
