namespace SSHVpnBot.Services.Excel.Models;


public class ServerReportsReportModel
{
    public string Code { get; set; }
    public string ClientId { get; set; }
    public string Url { get; set; }
    public string Server { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; }
    public string AnsweredBy { get; set; }
    public string AccountState { get; set; }

    public string Description { get; set; }
    public string Operator { get; set; }
    public string Type { get; set; }
    public string State { get; set; }
    public string LastModifiedDate { get; set; }
    public string CreatedOn { get; set; }
}