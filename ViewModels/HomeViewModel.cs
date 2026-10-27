using Oblachko.Models;

namespace Oblachko.ViewModels;

public class HomeViewModel
{
    public DateTime Now { get; set; }
    public List<OvenItem> Oven { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public Category? SelectedCategory { get; set; }
    public List<Product> Products { get; set; } = new();
    public List<Product> All { get; set; } = new();
}
