using System.Globalization;
using PersianDate.Standard;

namespace ConnectBashBot.Commons;

public static class DateTimeExtension
{
    public static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static float ToEpochTime(this DateTime dateTime)
    {
        var date = dateTime.ToUniversalTime();
        var ticks = date.Ticks - Epoch.Ticks;
        var ts = ticks / TimeSpan.TicksPerSecond;
        return (float)ts;
    }

    public static string ToPersianDate(this DateTime dateTime)
    {
        var pc = new PersianCalendar();
        var date = string.Format("{0}/{1}/{2}", pc.GetYear(dateTime), pc.GetMonth(dateTime),
            pc.GetDayOfMonth(dateTime));
        return date;
    }

    public static string ToConcatedDate(this DateTime date)
    {
        return date.Year + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0') +
               date.Hour.ToString().PadLeft(2, '0') + date.Minute.ToString().PadLeft(2, '0') +
               date.Second.ToString().PadLeft(2, '0');
    }

    public static DateTime FromConcatedDate(this string date)
    {
        return new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)),
            Convert.ToInt32(date.Substring(6, 2)), Convert.ToInt32(date.Substring(8, 2)),
            Convert.ToInt32(date.Substring(10, 2)), Convert.ToInt32(date.Substring(12, 2)));
    }

    public static long ConvertToTimestamp(this DateTime value)
    {
        var elapsedTime = value - Epoch;
        return (long)elapsedTime.TotalSeconds;
    }

    public static DateTime DateTimeFromUnixTimestampMillis(this float millis)
    {
        return Epoch.AddMilliseconds(millis);
    }

    public static string ConvertToPersianCalendar(this DateTime date)
    {
        var persian = date.ToPersianDate();
        return persian.ToEn().ToFa("ddd d MMM yyyy").En2Fa();
    }

    public static DateTime ConvertToDate(this float millis)
    {
        return Epoch.AddMilliseconds(millis);
    }

    public static string ConvertToPersianCalendar(this float millis)
    {
        var persian = Epoch.AddMilliseconds(millis).ToPersianDate();
        try
        {
            var res = persian.ToEn().ToFa("ddd d MMM yyyy hh:mm").En2Fa();
            return res;
        }
        catch (Exception e)
        {
            return "نامشخص";
        }
    }

    public static DateTime GetDateTimeFromIsoString(this string value)
    {
        if (!value.HasValue()) throw new ArgumentNullException(nameof(value));
        return DateTime.ParseExact(value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture);
    }
}