using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Oblachko.Models;

namespace Oblachko.ViewModels;

public class OrderViewModel
{
    public int ProductId { get; set; }

    [Range(1, 50, ErrorMessage = "От 1 до 50 штук")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; } = 1;

    [Required(ErrorMessage = "Укажите дату")]
    [Display(Name = "Когда заберёте")]
    public string PickupDate { get; set; } = "";

    [StringLength(200, ErrorMessage = "Не длиннее 200 символов")]
    [Display(Name = "Пожелания")]
    public string? Comment { get; set; }

    [BindNever]
    public Product? Product { get; set; }
}
