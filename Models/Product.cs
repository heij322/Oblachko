using System.ComponentModel.DataAnnotations;

namespace Oblachko.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    [StringLength(80, ErrorMessage = "Не длиннее 80 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = "";

    [StringLength(300, ErrorMessage = "Не длиннее 300 символов")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [StringLength(30, ErrorMessage = "Не длиннее 30 символов")]
    [Display(Name = "Вес")]
    public string? Weight { get; set; }

    [Range(1, 100000, ErrorMessage = "Цена от 1 до 100 000 ₽")]
    [Display(Name = "Цена, ₽")]
    public int Price { get; set; }

    public string? PhotoUrl { get; set; }

    [Display(Name = "Время выпечки")]
    public TimeOnly? BakeTime { get; set; }

    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
