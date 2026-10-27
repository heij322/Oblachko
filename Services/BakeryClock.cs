namespace Oblachko.Services;

public static class BakeryClock
{
    private const int IzhevskUtcOffsetHours = 4;

    public static DateTime Now => DateTime.UtcNow.AddHours(IzhevskUtcOffsetHours);

    public static DateOnly Today => DateOnly.FromDateTime(Now);

    public static DateTime ToLocal(DateTime utc) => utc.AddHours(IzhevskUtcOffsetHours);
}
