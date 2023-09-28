namespace SSHVpnBot.Components.Locations;

public class Location
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Code { get; set; }
    public string Flat { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; }

    public static string GenerateNewCode()
    {
        return "LOC" + DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
    }
}