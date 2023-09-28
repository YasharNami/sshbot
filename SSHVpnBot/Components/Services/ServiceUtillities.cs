using ConnectBashBot.Commons;

namespace SSHVpnBot.Components.Services;

public static class ServiceUtillities
{
    public static string GetFullTitle(this Service service)
    {
        var traffic = service.Traffic == 0 ? "نامحدود" : $"{service.Traffic.ToString().En2Fa()} گیگ";
        var month = service.Duration.Equals(30) ? "یک ماهه" :
            service.Duration.Equals(60) ? "دو ماهه" :
            service.Duration.Equals(90) ? "سه ماهه" :
            service.Duration.Equals(180) ? "شش ماهه" : service
                .Duration.ToString().En2Fa() + " روزه";

        var users = service.UserLimit.Equals(1) ? "تک کاربره" :
            service.UserLimit.Equals(2) ? "دو کاربره" : $"{service.UserLimit.ToString().En2Fa()} کاربره";

        return $"{month} {users} {traffic}";
    }

    public static string GetFullTitle(int days, int traffic, int user_limit)
    {
        var gig = traffic == 0 ? "نامحدود" : $"{traffic.ToString().En2Fa()} گیگ";
        var month = days.Equals(30) ? "یک ماهه" :
            days.Equals(60) ? "دو ماهه" :
            days.Equals(90) ? "سه ماهه" :
            days.Equals(180) ? "شش ماهه" : days.ToString().En2Fa() + " روزه";

        var users = user_limit.Equals(1) ? "تک کاربره" :
            user_limit.Equals(2) ? "دو کاربره" : $"{user_limit.ToString().En2Fa()} کاربره";

        return $"{month} {users} {gig}";
    }

    public static int CalcultatePrivateServicePrice(int days, int traffic, int user_limit)
    {
        var total = 0;
        switch (days)
        {
            case 30:
                total = 4000 * traffic;
                break;
            case 60:
                total = 5400 * traffic;
                break;
            case 90:
                total = 5900 * traffic;
                break;
            case 180:
                total = 6200 * traffic;
                break;
        }


        if (user_limit == 2)
            total += total + total / 100 * 30;

        return total;
    }

    public static int CalcultatePrivateServicePriceByDays(int days)
    {
        switch (days)
        {
            case 30:
                return 4000;
            case 60:
                return 5400;
            case 90:
                return 5900;
            case 180:
                return 6300;
            default:
                return 3000;
        }
    }
}
