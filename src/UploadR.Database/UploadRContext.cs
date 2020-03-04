using Microsoft.EntityFrameworkCore;
using UploadR.Database.Enums;
using UploadR.Database.Models;

namespace UploadR.Database
{
    public class UploadRContext : DbContext
    {
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string _connectionString;

        public UploadRContext(ConnectionStringProvider csp)
        {
            _connectionString = csp.ConnectionString;
        }

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
                .IsRequired()
                .HasColumnName("email");

            modelBuilder.Entity<User>()
                .Property(x => x.Disabled)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("disabled");

            modelBuilder.Entity<User>()
                .Property(x => x.Type)
                .IsRequired()
                .HasDefaultValue(AccountType.User)
                .HasColumnName("account_type");

            modelBuilder.Entity<User>()
                .Property(x => x.Token)
                .IsRequired()
                .HasColumnName("api_token_hash");

            modelBuilder.Entity<User>()
                .HasMany(x => x.Uploads)
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
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("download_count");

            modelBuilder.Entity<Upload>()
                .Property(x => x.Removed)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("removed");

            modelBuilder.Entity<Upload>()
                .Property(x => x.FileName)
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
        }
    }
}
