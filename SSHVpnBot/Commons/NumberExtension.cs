using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ConnectBashBot.Commons;

public static class NumberExtension
{
    public static decimal RoundToMoney(this decimal value, int @decimal = 2)
    {
        return Math.Round(value, @decimal, MidpointRounding.ToEven);
    }

    public static string En2Fa(this decimal str)
    {
        return str.ToString().Replace("0", "۰")
            .Replace("1", "۱")
            .Replace("2", "۲")
            .Replace("3", "۳")
            .Replace("4", "۴")
            .Replace("5", "۵")
            .Replace("6", "۶")
            .Replace("7", "۷")
            .Replace("8", "۸")
            .Replace("9", "۹");
    }

    public static decimal CheckMoney(this decimal value, string exMessage)
    {
        if (value > 0)
            return value;
        throw new ValidationException(exMessage);
    }

    public static string ToIranCurrency(this decimal Value)
    {
        return Value.ToString("N0", CultureInfo.InvariantCulture);
    }
    public static string ToIranCurrency(this float Value)
    {
        return Value.ToString("N0", CultureInfo.InvariantCulture);
    }


    public static string ToIranCurrency(this long Value)
    {
        return Value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string ToIranCurrency(this int Value)
    {
        return Value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string ToIranCurrency(this double Value)
    {
        return Value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string ToCommaSeparated(this decimal value, string format = "N")
    {
        return value.ToString("N", CultureInfo.InvariantCulture);
    }

    public static decimal ToDecimalInvariantCulture(this string value)
    {
        if (value == "?")
            return 0;

        var matchString = Regex.Match(value, "[\\d.]+");
        return Convert.ToDecimal(matchString.Value, CultureInfo.InvariantCulture);
    }

    public static decimal ChangeDecimal(this float digit, int decimalno)
    {
        var s = digit.ToString(CultureInfo.InvariantCulture);
        var res = s.Split('.');
        var section1 = res[0];
        if (res.Length > 1)
        {
            if (decimalno > res[1].Length) decimalno = res[1].Length;

            var t = res[1].Length - decimalno;
            var section2 = res[1].Remove(decimalno, t);
            return Convert.ToDecimal(section1 + "." + section2, CultureInfo.InvariantCulture);
        }
        else
        {
            return Convert.ToDecimal(res[0], CultureInfo.InvariantCulture);
        }
    }

    public static decimal ChangeDecimal(this double digit, int decimalno)
    {
        var s = digit.ToString(CultureInfo.InvariantCulture);
        var res = s.Split('.');
        var section1 = res[0];
        if (res.Length > 1)
        {
            if (decimalno > res[1].Length) decimalno = res[1].Length;

            var t = res[1].Length - decimalno;
            var section2 = res[1].Remove(decimalno, t);
            return Convert.ToDecimal(section1 + "." + section2, CultureInfo.InvariantCulture);
        }
        else
        {
            return Convert.ToDecimal(res[0], CultureInfo.InvariantCulture);
        }
    }
    public static decimal ChangeDecimal(this decimal digit, int decimalno)
    {
        var s = digit.ToString(CultureInfo.InvariantCulture);
        var res = s.Split('.');
        var section1 = res[0];
        if (res.Length > 1)
        {
            if (decimalno > res[1].Length) decimalno = res[1].Length;

            var t = res[1].Length - decimalno;
            var section2 = res[1].Remove(decimalno, t);
            return Convert.ToDecimal(section1 + "." + section2, CultureInfo.InvariantCulture);
        }
        else
        {
            return Convert.ToDecimal(res[0], CultureInfo.InvariantCulture);
        }
    }

    public static float ChangeFloatDecimal(this float digit, int decimalno)
    {
        var s = digit.ToString(CultureInfo.InvariantCulture);
        var res = s.Split('.');
        var section1 = res[0];
        if (res.Length > 1)
        {
            if (decimalno > res[1].Length) decimalno = res[1].Length;

            var t = res[1].Length - decimalno;
            var section2 = res[1].Remove(decimalno, t);
            return (float)Convert.ToDecimal(section1 + "." + section2, CultureInfo.InvariantCulture);
        }
        else
        {
            return (float)Convert.ToDecimal(res[0], CultureInfo.InvariantCulture);
        }
    }

    public static decimal DecimalToUp(this decimal digit, int decimalno)
    {
        var multiplier = (decimal)Math.Pow(10, Convert.ToDouble(decimalno, CultureInfo.InvariantCulture));
        return Math.Ceiling(digit * multiplier) / multiplier;
    }

    public static decimal DecimalToDown(this decimal digit, int decimalno)
    {
        var power = Convert.ToDecimal(Math.Pow(10, decimalno));
        return Math.Floor(digit * power) / power;
    }

    public static bool IsNumber(this string value)
    {
        var regex = new Regex(@"^\d+$");
        var match = regex.Match(value);
        if (match.Success)
            return true;
        else
            return false;
    }

    public static decimal DecimalChecker(this decimal digit)
    {
        return digit.ToString(CultureInfo.InvariantCulture).Split('.')[1].Contains('1')
            ? digit.ToString(CultureInfo.InvariantCulture)
                .Split('.')[1].Split('1')[0].Length + 1
            : 0;
    }

    public static decimal Round(this decimal value, int fractional = 6)
    {
        return Math.Round(value, fractional, MidpointRounding.AwayFromZero);
    }
}