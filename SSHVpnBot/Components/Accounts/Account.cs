using System.ComponentModel.DataAnnotations;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;

namespace SSHVpnBot.Components.Accounts;

public class Account
{
    public Account()
    {
        CreatedOn = DateTime.Now;
    }

    public int Id { get; set; }
    public string ClientId { get; set; }
    public AccountType Type { get; set; }
    public long UserId { get; set; }
    public DateTime StartsOn { get; set; }
    public DateTime EndsOn { get; set; }
    public AccountState State { get; set; }
    public string Url { get; set; }
    public bool Sold { get; set; }
    public string ServerCode { get; set; }
    public int LimitIp { get; set; }
    public bool IsActive { get; set; }
    public bool IsRemoved { get; set; }
    public string OrderCode { get; set; }
    public string ServiceCode { get; set; }
    public int Port { get; set; }
    public double Traffic { get; set; }
    public double UsedTraffic { get; set; }
    public string Email { get; set; }
    public DateTime CreatedOn { get; set; }
    public string AccountCode { get; set; }
    public string Note { get; set; }
    public int ExtendNotifyCount { get; set; }
    public int BlockAlertCount { get; set; }
    

    public static string GenerateNewAccountCode()
    {
        return "AC" +
               DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
    }
}

public enum AccountState : int
{
    [Display(Name = "🔴 غیرفعال")] DeActive = 0,
    [Display(Name = "🟢 فعال")] Active,
    [Display(Name = "منقضی شده 🛑")] Expired,
    [Display(Name = "اتمام ترافیک 🛑")] Expired_Traffic,

    [Display(Name = "مسدود شده - اتصال تعداد دستگاه بیش از حد مجاز 🔴")]
    Blocked_Ip,
    [Display(Name = "مسدود شده 🛑")] Blocked
}

public enum AccountType : int
{
    Normal = 0,
    Check,
    Inited
}