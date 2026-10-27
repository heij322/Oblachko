using System.ComponentModel.DataAnnotations;

namespace Oblachko.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    [StringLength(50, ErrorMessage = "Не длиннее 50 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = "";

    [Display(Name = "Порядок в меню")]
    public int SortOrder { get; set; }

    public List<Product> Products { get; set; } = new();
}
