using Microsoft.EntityFrameworkCore;
using ShareY.Database.Enums;
using ShareY.Database.Models;

namespace ShareY.Database
{
    public class ShareYContext : DbContext
    {
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string _connectionString;

        public ShareYContext(ConnectionStringProvider csp)
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
                .HasName("pk_token_guid");

            modelBuilder.Entity<Token>()
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<Token>()
                .Property(x => x.TokenType)
                .IsRequired()
                .HasDefaultValue(TokenType.User)
                .HasColumnName("token_type");

            modelBuilder.Entity<Token>()
                .Property(x => x.UserGuid)
                .IsRequired()
                .HasColumnType("uuid")
                .HasColumnName("user_guid");

            modelBuilder.Entity<Token>()
                .HasIndex(x => x.UserGuid)
                .IsUnique()
                .HasName("index_user_id");

            modelBuilder.Entity<Token>()
                .HasOne(x => x.User)
                .WithOne(x => x.Token)
                .HasForeignKey<Token>(x => x.UserGuid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_token_userid");
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
                .Property(x => x.ViewCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("view_count");

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
                .HasOne(x => x.Author)
                .WithMany(x => x.Uploads)
                .HasForeignKey(x => x.AuthorGuid)
                .HasConstraintName("fkey_upload_authorid");
            #endregion
        }
    }
}
