using Oblachko.Models;

namespace Oblachko.ViewModels;

public class AdminViewModel
{
    public List<Order> Orders { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}
