using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oblachko.Data;
using Oblachko.Models;
using Oblachko.Services;
using Oblachko.ViewModels;

namespace Oblachko.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var model = new OrderViewModel
        {
            ProductId = product.Id,
            Product = product,
            PickupDate = BakeryClock.Today.AddDays(1).ToString("yyyy-MM-dd")
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderViewModel model)
    {
        var product = await _db.Products.FindAsync(model.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        model.Product = product;

        var today = BakeryClock.Today;
        if (!DateOnly.TryParseExact(model.PickupDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var pickupDate))
        {
            ModelState.AddModelError(nameof(model.PickupDate), "Укажите дату");
        }
        else if (pickupDate < today || pickupDate > today.AddDays(30))
        {
            ModelState.AddModelError(nameof(model.PickupDate), "Выберите дату с сегодняшнего дня и не дальше месяца");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var order = new Order
        {
            UserId = _userManager.GetUserId(User)!,
            ProductId = product.Id,
            Quantity = model.Quantity,
            Total = product.Price * model.Quantity,
            PickupDate = pickupDate,
            Comment = model.Comment,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatuses.Accepted
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    public async Task<IActionResult> My()
    {
        var userId = _userManager.GetUserId(User);

        var orders = await _db.Orders
            .Include(o => o.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != _userManager.GetUserId(User) && !User.IsInRole(SeedData.AdminRole))
        {
            return Forbid();
        }

        return View(order);
    }
}
