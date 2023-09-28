namespace SSHVpnBot.Services.Excel.Models;

public class SubscriberReportModel
{
    public long UserId { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public string JoinedOn { get; set; }
    public string Referral { get; set; }
    public string isActive { get; set; }
    public string Balance { get; set; }
    public int Orders { get; set; }

    public int Accounts { get; set; }
    // public string TotalPayment { get; set; }
}