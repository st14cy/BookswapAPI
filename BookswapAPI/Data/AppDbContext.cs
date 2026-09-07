using Microsoft.EntityFrameworkCore;
using BookswapAPI.Models;
using BookswapAPI.Models.Enum;

namespace BookswapAPI.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<FavoriteAdvertisement> FavoriteAdvertisements => Set<FavoriteAdvertisement>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users, Genres, Books, Sellers, Advertisements and relation tables.
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Login).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Login).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Author).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Genre)
                .WithMany()
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Seller>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Surname).HasMaxLength(100);
            entity.Property(x => x.Image).HasMaxLength(500);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.HasIndex(x => new { x.City, x.IsActive });
            entity.HasOne(x => x.Book)
                .WithMany()
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Seller)
                .WithMany(x => x.Advertisements)
                .HasForeignKey(x => x.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FavoriteAdvertisement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.AdvertisementId }).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Advertisement)
                .WithMany()
                .HasForeignKey(x => x.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SellerId, x.AdvertisementId }).IsUnique();
            entity.HasOne(x => x.Seller)
                .WithMany()
                .HasForeignKey(x => x.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Advertisement)
                .WithMany()
                .HasForeignKey(x => x.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}