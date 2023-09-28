namespace SSHVpnBot.Services.Excel.Models;

public class ColleagueReportModel
{
    public long UserId { get; set; }
    public int Accounts { get; set; }
    public long Balance { get; set; }

    public int Orders { get; set; }
    public long TotalPayments { get; set; }
    public string Tag { get; set; }
    public string JoinedOn { get; set; }
}