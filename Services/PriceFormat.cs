using System.Globalization;

namespace Oblachko.Services;

public static class PriceFormat
{
    private static readonly CultureInfo Russian = new("ru-RU");

    public static string Rub(this int price) => price.ToString("N0", Russian) + " ₽";
}
