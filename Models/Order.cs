using Microsoft.AspNetCore.Identity;

namespace Oblachko.Models;

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public IdentityUser? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public int Total { get; set; }
    public DateOnly PickupDate { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = OrderStatuses.Accepted;
}

public static class OrderStatuses
{
    public const string Accepted = "Принят";
    public const string Ready = "Готов";
    public const string Done = "Выдан";
    public const string Canceled = "Отменён";

    public static readonly string[] All = { Accepted, Ready, Done, Canceled };

    public static string CssClass(string status) => status switch
    {
        Ready => "stamp-ready",
        Done => "stamp-done",
        Canceled => "stamp-canceled",
        _ => "stamp-accepted"
    };
}
