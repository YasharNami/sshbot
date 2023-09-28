using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Discounts;


public class Discount
{
    public int Id { get; set; }
    public string DiscountNumber { get; set; }
    public string Code { get; set; }
    public DiscountType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpiredOn { get; set; }
    public int UsageLimitation { get; set; }
    public string ServiceCode { get; set; }
    public long UserId { get; set; }
    public int MaxAmountOfPercent { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; }
    public bool IsRemoved { get; set; }
    public int UsedCount { get; set; }

    public static string GenerateNewDiscountNumber()
    {
        return "D" + DateTime.UtcNow.Ticks.ToString().Substring(12, 6);
    }

}

public enum DiscountType : int
{
    [Display(Name = "درصدی")] Percent = 0,
    [Display(Name = "مقداری")] Amount
}