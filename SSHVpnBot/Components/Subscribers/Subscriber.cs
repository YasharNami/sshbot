using System.ComponentModel.DataAnnotations;
using SSHVpnBot.Domains;

namespace SSHVpnBot.Components.Subscribers;

public class Subscriber : Entity<int>
{
    public long UserId { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public Role Role { get; set; }
    public DateTime JoinedOn { get; set; }
    public string Referral { get; set; }
    public bool isActive { get; set; }
    public string Step { get; set; }
    public bool Notification { get; set; }
}

public enum Role : int
{
    [Display(Name = "کاربر عادی")] Subscriber = 0,
    [Display(Name = "فروشنده")] Colleague,
    [Display(Name = "ادمین")] Admin,
    [Display(Name = "مدیر کل")] Owner
}