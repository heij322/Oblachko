using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Oblachko.Data;
using Oblachko.Models;
using Oblachko.Services;
using Oblachko.ViewModels;

namespace Oblachko.Controllers;

[Authorize(Roles = SeedData.AdminRole)]
public class AdminController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxPhotoSize = 5 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly UploadStorage _uploads;

    public AdminController(AppDbContext db, UploadStorage uploads)
    {
        _db = db;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminViewModel
        {
            Orders = await _db.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)
                .ToListAsync(),
            Products = await _db.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Category!.SortOrder)
                .ThenBy(p => p.Name)
                .ToListAsync(),
            Categories = await _db.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        await FillCategoriesAsync();
        return View("ProductForm", new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product, IFormFile? photo)
    {
        await CheckCategoryAsync(product);
        await SavePhotoAsync(photo, product);

        if (!ModelState.IsValid)
        {
            await FillCategoriesAsync();
            return View("ProductForm", product);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Изделие «{product.Name}» добавлено";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        await FillCategoriesAsync();
        return View("ProductForm", product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Product product, IFormFile? photo)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == product.Id))
        {
            return NotFound();
        }

        await CheckCategoryAsync(product);
        await SavePhotoAsync(photo, product);

        if (!ModelState.IsValid)
        {
            await FillCategoriesAsync();
            return View("ProductForm", product);
        }

        _db.Products.Update(product);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Изделие «{product.Name}» сохранено";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Изделие «{product.Name}» удалено";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View("CategoryForm", new Category());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View("CategoryForm", category);
        }

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Категория «{category.Name}» добавлена";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View("CategoryForm", category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(Category category)
    {
        var existing = await _db.Categories.FindAsync(category.Id);
        if (existing == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("CategoryForm", category);
        }

        existing.Name = category.Name;
        existing.SortOrder = category.SortOrder;
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Категория «{existing.Name}» сохранена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            TempData["Error"] = $"В категории «{category.Name}» есть изделия. Сначала удалите или перенесите их.";
            return RedirectToAction(nameof(Index));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Категория «{category.Name}» удалена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetOrderStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order != null && OrderStatuses.All.Contains(status))
        {
            order.Status = status;
            await _db.SaveChangesAsync();
            TempData["Message"] = $"Заказ № {order.Id:D4}: статус «{status}»";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task FillCategoriesAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();

        ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name));
    }

    private async Task CheckCategoryAsync(Product product)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == product.CategoryId))
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Выберите категорию");
        }
    }

    private async Task SavePhotoAsync(IFormFile? photo, Product product)
    {
        if (photo == null || photo.Length == 0)
        {
            return;
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(string.Empty, "Фото должно быть в формате jpg, png или webp");
            return;
        }

        if (photo.Length > MaxPhotoSize)
        {
            ModelState.AddModelError(string.Empty, "Фото должно весить не больше 5 МБ");
            return;
        }

        var fileName = Guid.NewGuid().ToString("N") + extension;
        var path = Path.Combine(_uploads.Folder, fileName);

        await using (var stream = System.IO.File.Create(path))
        {
            await photo.CopyToAsync(stream);
        }

        product.PhotoUrl = "/uploads/" + fileName;
    }
}
