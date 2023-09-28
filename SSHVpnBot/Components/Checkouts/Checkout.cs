using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Checkouts;


public class Checkout
{
    public int Id { get; set; }
    public long Amount { get; set; }
    public string Code { get; set; }
    public long UserId { get; set; }
    public string IBan { get; set; }
    public string TransactionCode { get; set; }

    public DateTime CreatedOn { get; set; }
    public CheckoutState State { get; set; }

    public static string GenerateNewCheckoutCode()
    {
        return "C" + DateTime.UtcNow.Ticks.ToString().Substring(6, 12);

    }
}

public enum CheckoutState : int
{
    [Display(Name = "در انتظار تایید")] Pending = 0,
    [Display(Name = "تایید شده")] Approved,
    [Display(Name = "رد شده")] Declined
}