using SSHVpnBot.Domains;

namespace SSHVpnBot.Components.Services;

public class Service : Entity<int>
{
    public Service()
    {
        IsActive = false;
        IsRemoved = false;
        Duration = 0;
        Traffic = 0;
        Price = 0;
        Title = "تنظیم نشده";
        Description = "";
        UserLimit = 0;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Code { get; set; }
    public int Duration { get; set; }
    public float Traffic { get; set; }
    public int UserLimit { get; set; }
    public bool IsActive { get; set; }
    public bool IsRemoved { get; set; }
    public decimal SellerPrice { get; set; }

    public static string GenerateNewCode()
    {
        return "S" + DateTime.UtcNow.Ticks.ToString().Substring(12, 6);
    }
}