using Oblachko.Models;

namespace Oblachko.ViewModels;

public enum OvenStatus
{
    Done,
    Hot,
    Next
}

public class OvenItem
{
    private const int HotMinutes = 60;

    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string Time { get; set; } = "";
    public OvenStatus Status { get; set; }
    public string Note { get; set; } = "";

    public string CssClass => Status switch
    {
        OvenStatus.Done => "oven-done",
        OvenStatus.Hot => "oven-hot",
        _ => "oven-next"
    };

    public static OvenItem Create(Product product, TimeOnly now)
    {
        var bake = product.BakeTime ?? TimeOnly.MinValue;
        var minutes = (int)(now.ToTimeSpan() - bake.ToTimeSpan()).TotalMinutes;

        var item = new OvenItem
        {
            ProductId = product.Id,
            Name = product.Name,
            Time = bake.ToString("H:mm")
        };

        if (minutes < 0)
        {
            item.Status = OvenStatus.Next;
            item.Note = "через " + FormatMinutes(-minutes);
        }
        else if (minutes < HotMinutes)
        {
            item.Status = OvenStatus.Hot;
            item.Note = minutes < 1 ? "только что из печи" : $"горячий, {minutes} мин назад";
        }
        else
        {
            item.Status = OvenStatus.Done;
            item.Note = "уже на полке";
        }

        return item;
    }

    private static string FormatMinutes(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} мин";
        }

        return minutes % 60 == 0
            ? $"{minutes / 60} ч"
            : $"{minutes / 60} ч {minutes % 60} мин";
    }
}
