using System.Drawing;
using System.Globalization;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Net.Codecrete.QrCodeGenerator;

namespace ConnectBashBot.Commons;

public static class StringExtension
{
    /*private static readonly string passPhrase = "e547A572-6922-4830-88c2-9aava83c222a";
    private static readonly string saltValue = "d6b087s4-b681-45ce-9241-50a1cbc336f8";*/
    private const string EncryptionKey = "fe0505f4-4ev3-4592-8e80-cf2d8ec4795b";


    public static string Encrypt(this string clearText, string? salt = null)
    {
        var clearBytes = Encoding.Unicode.GetBytes(clearText);
        using var encryptor = Aes.Create();
        var pdb = new Rfc2898DeriveBytes(string.IsNullOrEmpty(salt) ? EncryptionKey : salt,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        encryptor.Key = pdb.GetBytes(32);
        encryptor.IV = pdb.GetBytes(16);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
        }

        clearText = Convert.ToBase64String(ms.ToArray());

        return clearText;
    }

    public static string Decrypt(this string cipherText, string? salt = null)
    {
        cipherText = cipherText.Replace(" ", "+");
        var cipherBytes = Convert.FromBase64String(cipherText);
        using var encryptor = Aes.Create();
        var pdb = new Rfc2898DeriveBytes(string.IsNullOrEmpty(salt) ? EncryptionKey : salt,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        encryptor.Key = pdb.GetBytes(32);
        encryptor.IV = pdb.GetBytes(16);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();
        }

        cipherText = Encoding.Unicode.GetString(ms.ToArray());

        return cipherText;
    }


    /// <summary>
    /// Checks the input NationalCode With NationalCode Generation Algorithm
    /// </summary>
    /// <returns></returns>
    public static bool IsValidNationalCode(this string input)
    {
        try
        {
            var chArray = input.ToCharArray();
            var numArray = new int[chArray.Length];
            for (var i = 0; i < chArray.Length; i++) numArray[i] = (int)char.GetNumericValue(chArray[i]);

            var num2 = numArray[9];
            var num3 =
                numArray[0] * 10 + numArray[1] * 9 + numArray[2] * 8 + numArray[3] * 7 +
                numArray[4] * 6 +
                numArray[5] * 5 + numArray[6] * 4 + numArray[7] * 3 + numArray[8] * 2;
            var num4 = num3 - num3 / 11 * 11;
            return (num4 == 0 && num2 == num4) || (num4 == 1 && num2 == 1) ||
                   (num4 > 1 && num2 == Math.Abs((int)(num4 - 11)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static DateTime PersianDateStringToDateTime(this string? persianDate)
    {
        if (string.IsNullOrEmpty(persianDate))
            return DateTime.Now;

        var pc = new PersianCalendar();
        var persianDateSplitedParts = persianDate.Split('/');
        var dateTime = new DateTime(int.Parse(persianDateSplitedParts[0]), int.Parse(persianDateSplitedParts[1]),
            int.Parse(persianDateSplitedParts[2]), pc);
        return DateTime.Parse(dateTime.ToString(CultureInfo.CreateSpecificCulture("en-US")));
    }

    public static string HideBankAccountCardNumber(this string input)
    {
        return input[..4] + "********" + input.Substring(12, 4);
    }

    public static bool HasValue(this string? value, bool ignoreWhiteSpace = true)
    {
        return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
    }

    public static string En2Fa(this string str)
    {
        return str.Replace("0", "۰")
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
  
    public static string En2Fa(this int str)
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
    public static void CopyTo(Stream src, Stream dest)
    {
        var bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) dest.Write(bytes, 0, cnt);
    }


    public static string Fa2En(this string str)
    {
        return str.Replace("۰", "0")
            .Replace("۱", "1")
            .Replace("۲", "2")
            .Replace("۳", "3")
            .Replace("۴", "4")
            .Replace("۵", "5")
            .Replace("۶", "6")
            .Replace("۷", "7")
            .Replace("۸", "8")
            .Replace("۹", "9")
            .Replace("٠", "0")
            .Replace("١", "1")
            .Replace("٢", "2")
            .Replace("٣", "3")
            .Replace("٤", "4")
            .Replace("٥", "5")
            .Replace("٦", "6")
            .Replace("٧", "7")
            .Replace("٨", "8")
            .Replace("٩", "9");
    }


    public static string FixPersianChars(this string str)
    {
        return str.Replace("ﮎ", "ک")
            .Replace("ﮏ", "ک")
            .Replace("ﮐ", "ک")
            .Replace("ﮑ", "ک")
            .Replace("ك", "ک")
            .Replace("ي", "ی")
            .Replace(" ", " ")
            .Replace("‌", " ")
            .Replace("ھ", "ه");
    }

    public static string GenerateNumber(int count = 6)
    {
        var random = new Random();
        return new string(Enumerable.Repeat("0123456789", count)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GenerateRandomString(int count = 12)
    {
        var random = new Random();
        return new string(Enumerable.Repeat("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM", count)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    //public static string ToMd5String(this string str)
    //{
    //    Byte[] originalBytes;
    //    Byte[] encodedBytes;
    //    MD5 md5;
    //    md5 = new MD5CryptoServiceProvider();
    //    originalBytes = ASCIIEncoding.Default.GetBytes(str);
    //    encodedBytes = md5.ComputeHash(originalBytes);
    //    return BitConverter.ToString(encodedBytes);
    //}

    public static Guid ToGuid(this string guid)
    {
        return Guid.Parse(guid);
    }

    public static bool IsGuid(this string value)
    {
        Guid x;
        return Guid.TryParse(value, out x);
    }
}