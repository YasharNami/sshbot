using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.AccountReports;

public class AccountReport
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string AccountCode { get; set; }
    public string ServerCode { get; set; }
    public long UserId { get; set; }
    public long AnsweredBy { get; set; }
    public string Description { get; set; }
    public AccountOperator Operator { get; set; }
    public AccountReportType Type { get; set; }
    public ReportState State { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public DateTime CreatedOn { get; set; }

    public static string GenerateNewAccountCode()
    {
        return "REP" + DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
    }
}
public enum AccountOperator : int
{
    [Display(Name = "ایرانسل")] Irancell = 0,
    [Display(Name = "همراه اول")] MCI,
    [Display(Name = "رایتل")] Rightel,
    [Display(Name = "وایفای")] WIFI
}
public enum AccountReportType : int
{
    [Display(Name = "گزارش کندی")] LowSpeed = 0,
    [Display(Name = "گزارش قطعی")] Disconnection
}
public enum ReportState : int
{
    [Display(Name = "باز")] Open = 0,
    [Display(Name = "در حال بررسی")] Checking,
    [Display(Name = "بررسی شده")] Checked,
    [Display(Name = "پاسخ داده شده")] Answered,
    [Display(Name = "برطرف شده")] Fixed
}