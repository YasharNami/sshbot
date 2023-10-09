using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Servers;

public class Server
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Url { get; set; }
    public string Domain { get; set; }
    public bool IsActive { get; set; }
    public ServerType Type { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Session { get; set; }
    public DateTime CreatedOn { get; set; }
    public int Capacity { get; set; }
    public bool IsRemoved { get; set; }
    public string Note { get; set; }
    public string SSHPassword { get; set; }
    public string LocationCode { get; set; }

    public static string GenerateNewCode()
    {
        return "SERVER" + DateTime.UtcNow.Ticks.ToString().Substring(12, 6);
    }
}
public enum ServerType : int
{
    [Display(Name = "سرور اصلی ⚜️")] Main = 0,
    [Display(Name = "سرور تست 🪛")] Check,
    // [Display(Name = "سرور همکار 👨‍💻")] Colleague,
    //
    // [Display(Name = "سرور پرزنت حضوری 👀")]
    // Peresent
}