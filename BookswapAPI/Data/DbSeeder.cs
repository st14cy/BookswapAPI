using BookswapAPI.Data;
using BookswapAPI.Models;
using BookswapAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return;

        var now = DateTime.UtcNow;

        // ---------- Users ----------
        var users = new List<User>
        {
            new() {
                Id = Guid.NewGuid(),
                Login = "alice",
                Email = "alice@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
                Role = Roles.User,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Login = "bob",
                Email = "bob@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
                Role = Roles.User,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Login = "carol",
                Email = "carol@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
                Role = Roles.User,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Login = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = Roles.Admin,
                CreatedAt = now
            },
        };
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // ---------- Genres ----------
        var genreNames = new[]
        {
            "Фантастика", "Фэнтези", "Детектив", "Роман",
            "Научная литература", "История", "Поэзия", "Ужасы"
        };
        var genres = genreNames.Select(n => new Genre
        {
            Id = Guid.NewGuid(),
            Name = n,
            CreatedAt = now
        }).ToList();
        db.Genres.AddRange(genres);
        await db.SaveChangesAsync();

        // ---------- Sellers ----------
        var sellers = new List<Seller>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Алиса",
                Rating = 4.8,
                Image = "https://i.pravatar.cc/150?img=1",
                UserId = users[0].Id,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Борис",
                Rating = 4.5,
                Image = "https://i.pravatar.cc/150?img=2",
                UserId = users[1].Id,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Карина",
                Rating = 4.9,
                Image = "https://i.pravatar.cc/150?img=3",
                UserId = users[2].Id,
                CreatedAt = now
            },
        };
        db.Sellers.AddRange(sellers);
        await db.SaveChangesAsync();

        // ---------- Advertisements ----------
        // GenreId — обязателен, Author — обязателен, TitleBook — обязателен
        var ads = new List<Advertisement>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Продам классику",
                TitleBook = "Война и мир. Том 1",
                Author = "Лев Толстой",
                GenreId = genres[3].Id, // Роман
                SellerId = sellers[0].Id,
                Description = "Классика, состояние отличное",
                City = "Москва",
                Street = "Тверская",
                HouseNumber = "1",
                IsActive = true,
                IsNew = false,
                IsForever = false,
                IsPostamat = true,
                StartDate = now,
                EndDate = now.AddMonths(1),
                ViewsCount = 120,
                LikeCount = 15,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Отдам в хорошие руки",
                TitleBook = "Преступление и наказание",
                Author = "Фёдор Достоевский",
                GenreId = genres[3].Id,
                SellerId = sellers[0].Id,
                Description = "Мягкая обложка, следы времени",
                City = "Москва",
                Street = "Арбат",
                HouseNumber = "5",
                IsActive = true,
                IsNew = false,
                IsForever = false,
                IsPostamat = false,
                StartDate = now,
                EndDate = now.AddMonths(2),
                ViewsCount = 88,
                LikeCount = 7,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Подарочное издание",
                TitleBook = "Мастер и Маргарита",
                Author = "Михаил Булгаков",
                GenreId = genres[3].Id,
                SellerId = sellers[1].Id,
                Description = "Твёрдый переплёт, иллюстрации",
                City = "Санкт-Петербург",
                Street = "Невский проспект",
                HouseNumber = "10",
                IsActive = true,
                IsNew = true,
                IsForever = true,
                IsPostamat = false,
                StartDate = now,
                EndDate = now.AddYears(1),
                ViewsCount = 210,
                LikeCount = 30,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Антиутопия",
                TitleBook = "1984",
                Author = "Джордж Оруэлл",
                GenreId = genres[0].Id, // Фантастика
                SellerId = sellers[1].Id,
                Description = "Перевод Д. Волкогонова",
                City = "Санкт-Петербург",
                Street = "Литейный",
                HouseNumber = "3",
                IsActive = true,
                IsNew = false,
                IsForever = false,
                IsPostamat = true,
                StartDate = now,
                EndDate = now.AddMonths(1),
                ViewsCount = 175,
                LikeCount = 22,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Первое издание",
                TitleBook = "Гарри Поттер и философский камень",
                Author = "Дж. К. Роулинг",
                GenreId = genres[1].Id, // Фэнтези
                SellerId = sellers[2].Id,
                Description = "Состояние идеальное",
                City = "Казань",
                Street = "Баумана",
                HouseNumber = "7",
                IsActive = true,
                IsNew = true,
                IsForever = false,
                IsPostamat = true,
                StartDate = now,
                EndDate = now.AddMonths(3),
                ViewsCount = 340,
                LikeCount = 50,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Фантастика, новая",
                TitleBook = "Дюна",
                Author = "Фрэнк Герберт",
                GenreId = genres[0].Id,
                SellerId = sellers[2].Id,
                Description = "Не читалась",
                City = "Казань",
                Street = "Кремлёвская",
                HouseNumber = "2",
                IsActive = true,
                IsNew = true,
                IsForever = false,
                IsPostamat = false,
                StartDate = now,
                EndDate = now.AddMonths(1),
                ViewsCount = 95,
                LikeCount = 12,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Все рассказы",
                TitleBook = "Шерлок Холмс. Сборник",
                Author = "Артур Конан Дойл",
                GenreId = genres[2].Id, // Детектив
                SellerId = sellers[0].Id,
                Description = "Полное собрание",
                City = "Новосибирск",
                Street = "Красный проспект",
                HouseNumber = "100",
                IsActive = false,
                IsNew = false,
                IsForever = false,
                IsPostamat = false,
                StartDate = now,
                EndDate = now.AddMonths(2),
                ViewsCount = 60,
                LikeCount = 5,
                CreatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                Title = "Ремарк, твёрдый переплёт",
                TitleBook = "Три товарища",
                Author = "Эрих Мария Ремарк",
                GenreId = genres[3].Id,
                SellerId = sellers[1].Id,
                Description = "В отличном состоянии",
                City = "Москва",
                Street = "Пятницкая",
                HouseNumber = "20",
                IsActive = true,
                IsNew = false,
                IsForever = false,
                IsPostamat = true,
                StartDate = now,
                EndDate = now.AddMonths(1),
                ViewsCount = 140,
                LikeCount = 18,
                CreatedAt = now
            },
        };
        db.Advertisements.AddRange(ads);
        await db.SaveChangesAsync();

        // ---------- Favorites ----------
        var favorites = new List<FavoriteAdvertisement>
        {
            new() { Id = Guid.NewGuid(), UserId = users[0].Id, AdvertisementId = ads[2].Id, AddedAt = now, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = users[0].Id, AdvertisementId = ads[3].Id, AddedAt = now, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = users[1].Id, AdvertisementId = ads[0].Id, AddedAt = now, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = users[1].Id, AdvertisementId = ads[4].Id, AddedAt = now, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = users[2].Id, AdvertisementId = ads[5].Id, AddedAt = now, CreatedAt = now },
            new() { Id = Guid.NewGuid(), UserId = users[3].Id, AdvertisementId = ads[1].Id, AddedAt = now, CreatedAt = now },
        };
        db.FavoriteAdvertisements.AddRange(favorites);
        await db.SaveChangesAsync();
    }
}