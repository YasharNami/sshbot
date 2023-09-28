namespace SSHVpnBot.Components.ServiceCategories;

public class ServiceCategory
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }  
    public bool IsActive { get; set; } 
    public bool IsRemoved { get; set; }
    public static string GenerateNewCode() => ("CAT") + DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
}