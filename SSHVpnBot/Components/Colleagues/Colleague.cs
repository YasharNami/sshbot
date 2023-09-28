using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Colleagues;


public class Colleague
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Tag { get; set; }
    public ColleagueLevel Level { get; set; }
    
    public AverageOrder AverageOrder { get; set; }

    public HowMeetUs HowMeetUs { get; set; }
    public HowToSell HowToSell { get; set; }
    public ColleagueStage Stage { get; set; }
    public DateTime JoinedOn { get; set; }
}

public enum ColleagueStage : int
{
    FullName = 0,
    PhoneNumber,
    HowToSell,
    AverageOrder,
    HowMeetUs,
    Done
}
public enum HowToSell : int
{
    [Display(Name = "مغازه دار هستم ✋🏻")]
    OfflineShop = 1,
    [Display(Name = "بصورت آنلاین فعالیت میکنم ✋🏻")]
    OnlineShop,
    [Display(Name = "بازاریاب هستم ✋🏻")]
    Networker
}
public enum HowMeetUs : int
{
    [Display(Name = "دوستان و آشنایان 🗣️")]
    Friends = 1,
    [Display(Name = "تبلیغات آنلاین 📣")]
    Ads
}
public enum AverageOrder :int
{
    [Display(Name = "زیر ۵ کانفیگ در روز 🔗")]
    LessThan5 = 1,
    [Display(Name = "۵ الی ۱۰ کانفیگ در روز 🔗")]
    FiveToTen,
    [Display(Name = "۱۰ الی ۲۰ کانفیگ در روز 🔗")]
    TenToTwenty,
    [Display(Name = "بیش از ۲۰ کانفیگ در روز 🔗")]
    MoreThanTwenty
}


public enum ColleagueLevel : int
{
    [Display(Name = "سطح پایه")] Base = 0,
    [Display(Name = "سطح برنزی")] Bronze,
    [Display(Name = "سطح نقره ای")] Silver,
    [Display(Name = "سطح طلایی")] Gold,
    [Display(Name = "سطح حرفه ای")] Pro
}