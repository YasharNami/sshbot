using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Payments;

public class Payment
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public PaymentState State { get; set; }
    public PaymentType Type { get; set; }

    public bool IsRemoved { get; set; }
    public string PaymentCode { get; set; }

    public static string GenerateNewPaymentCode()
    {
        return "P" +               
               DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
    }
}

public enum PaymentState : int
{
    [Display(Name = "در انتظار تایید")] Pending = 0,
    [Display(Name = "تایید شده")] Approved,
    [Display(Name = "رد شده")] Declined
}
public enum PaymentType : int
{
    Cart = 0,
    Gateway,
    Crypto,
    Wallet
}