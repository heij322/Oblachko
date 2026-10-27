using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oblachko.Data;
using Oblachko.Services;
using Oblachko.ViewModels;

namespace Oblachko.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? category)
    {
        var categories = await _db.Categories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var selected = categories.FirstOrDefault(c => c.Id == category) ?? categories.FirstOrDefault();

        var products = await _db.Products
            .OrderBy(p => p.Name)
            .ToListAsync();

        var now = BakeryClock.Now;

        var model = new HomeViewModel
        {
            Now = now,
            Categories = categories,
            SelectedCategory = selected,
            Products = selected == null
                ? new()
                : products.Where(p => p.CategoryId == selected.Id).ToList(),
            All = products,
            Oven = products
                .Where(p => p.BakeTime != null)
                .OrderBy(p => p.BakeTime)
                .Select(p => OvenItem.Create(p, TimeOnly.FromDateTime(now)))
                .ToList()
        };

        return View(model);
    }

    public IActionResult Status(int id)
    {
        ViewData["Code"] = id;
        return View();
    }
}
