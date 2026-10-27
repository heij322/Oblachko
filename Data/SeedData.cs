using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Oblachko.Models;

namespace Oblachko.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var email = config["Admin:Email"] ?? "admin@oblachko.ru";
        var password = config["Admin:Password"] ?? "admin123";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var created = await userManager.CreateAsync(admin, password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException("Не удалось создать администратора: "
                    + string.Join(", ", created.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
        }

        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var bread = new Category { Name = "Хлеб", SortOrder = 1 };
        var pastry = new Category { Name = "Выпечка", SortOrder = 2 };
        var desserts = new Category { Name = "Десерты", SortOrder = 3 };
        var cakes = new Category { Name = "Торты на заказ", SortOrder = 4 };
        db.Categories.AddRange(bread, pastry, desserts, cakes);

        db.Products.AddRange(
            new Product { Category = bread, Name = "Багет на закваске", Description = "Хрустящая корка, плотный пористый мякиш", Weight = "280 г", Price = 120, PhotoUrl = "/img/products/baguette.jpg", BakeTime = new TimeOnly(7, 0) },
            new Product { Category = bread, Name = "Бородинский", Description = "Ржаной, с кориандром и солодом", Weight = "400 г", Price = 95, PhotoUrl = "/img/products/borodinsky.jpg", BakeTime = new TimeOnly(10, 0) },
            new Product { Category = bread, Name = "Чиабатта", Description = "На оливковом масле, с крупными порами", Weight = "300 г", Price = 110, PhotoUrl = "/img/products/ciabatta.jpg", BakeTime = new TimeOnly(12, 0) },
            new Product { Category = bread, Name = "Цельнозерновой", Description = "Без сахара, на ржаной закваске", Weight = "500 г", Price = 130, PhotoUrl = "/img/products/wholegrain.jpg" },
            new Product { Category = pastry, Name = "Круассан", Description = "Сливочное масло 82%, много тонких слоёв", Weight = "70 г", Price = 140, PhotoUrl = "/img/products/croissant.jpg", BakeTime = new TimeOnly(8, 30) },
            new Product { Category = pastry, Name = "Синнабон", Description = "Корица и сливочный крем", Weight = "120 г", Price = 160, PhotoUrl = "/img/products/cinnabon.jpg", BakeTime = new TimeOnly(10, 30) },
            new Product { Category = pastry, Name = "Пирожок с капустой", Description = "Дрожжевое тесто, тушёная капуста", Weight = "90 г", Price = 70, PhotoUrl = "/img/products/pirozhok.jpg", BakeTime = new TimeOnly(16, 0) },
            new Product { Category = pastry, Name = "Ватрушка", Description = "Творог и ваниль", Weight = "100 г", Price = 85, PhotoUrl = "/img/products/vatrushka.jpg" },
            new Product { Category = desserts, Name = "Эклер", Description = "Заварной крем, шоколадная глазурь", Weight = "80 г", Price = 120, PhotoUrl = "/img/products/eclair.jpg" },
            new Product { Category = desserts, Name = "Медовик, кусок", Description = "Сметанный крем, гречишный мёд", Weight = "150 г", Price = 190, PhotoUrl = "/img/products/medovik.jpg" },
            new Product { Category = desserts, Name = "Картошка", Description = "Как в детстве, с какао", Weight = "70 г", Price = 90, PhotoUrl = "/img/products/kartoshka.jpg" },
            new Product { Category = cakes, Name = "Медовик целый", Description = "Заказывайте за два дня", Weight = "1,5 кг", Price = 1600, PhotoUrl = "/img/products/medovik.jpg" },
            new Product { Category = cakes, Name = "Наполеон", Description = "Заказывайте за два дня", Weight = "1,5 кг", Price = 1800, PhotoUrl = "/img/products/napoleon.jpg" }
        );

        await db.SaveChangesAsync();
    }
}
