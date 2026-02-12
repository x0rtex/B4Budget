namespace B4Budget.Services;

public static class DateHelper
{
    private static readonly string[] MonthAbbreviations =
    [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];

    public static string GetMonthLabel(int startMonth, int startYear, int monthOffset)
    {
        var (month, year) = GetCalendarMonthYear(startMonth, startYear, monthOffset);
        return $"{MonthAbbreviations[month - 1]} {year % 100:D2}";
    }

    public static (int Month, int Year) GetCalendarMonthYear(int startMonth, int startYear, int monthOffset)
    {
        var totalMonths = (startMonth - 1) + monthOffset;
        var year = startYear + (totalMonths / 12);
        var month = (totalMonths % 12) + 1;
        return (month, year);
    }
}