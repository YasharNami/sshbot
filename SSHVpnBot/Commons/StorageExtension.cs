namespace ConnectBashBot.Commons;

public static class StorageExtension
{
    public static string ByteToGB(this int value)
    {
        return ((float)value / 2024 / 2024 * 3.82f / 1000).ChangeDecimal(2).ToString();
    }

    public static string MegaByteToGB(this int value)
    {
        return ((float)value / 1000).ChangeDecimal(2).ToString();
    }
    public static double MegaByteToGB(this double value)
    {
        return (double)((double)value / 1000).ChangeDecimal(2);
    }
    public static decimal MegaByteToGB(this decimal value)
    {
        return ((float)value / 1000).ChangeDecimal(2);
    }
    public static decimal MegaByteToGB(this float value)
    {
        return (value / 1000).ChangeDecimal(2);
    }
    public static string ByteToGB(this float value)
    {
        return (value / 2024 / 2024 * 3.82f / 1000).ChangeDecimal(2).ToString();
    }

    public static string ByteToGB(this decimal value)
    {
        return ((float)value / 2024 / 2024 * 3.82f / 1000).ChangeDecimal(2).ToString();
    }

    public static string ByteToGB(this double value)
    {
        return ((float)value / 2024 / 2024 * 3.82f / 1000).ChangeDecimal(2).ToString();
    }

    public static string GBToByte(this int value)
    {
        return ((float)value * 1024 * 1024 * 1024).ToString();
    }

    public static string GBToByte(this double value)
    {
        return ((float)value * 1024 * 1024 * 1024).ToString();
    }

    public static double GBToDoubleByte(this double value)
    {
        return (float)value * 1024 * 1024 * 1024;
    }

    public static string GBToByte(this float value)
    {
        return (value * 1024 * 1024 * 1024).ToString();
    }

    public static decimal GBToDecimalByte(this int value)
    {
        return value * 1024 * 1024 * 1024;
    }

    public static float GBToFloatByte(this float value)
    {
        return value * 1024 * 1024 * 1024;
    }

    public static string GBToByte(this decimal value)
    {
        return ((float)value * 1024 * 1024 * 1024).ToString();
    }
}