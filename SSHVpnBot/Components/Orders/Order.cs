using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Orders;

public class Order
{
    public Order()
    {
        CreatedOn = DateTime.Now;
    }

    public int Id { get; set; }
    public string TrackingCode { get; set; }
    public string AccountCode { get; set; }
    public DateTime CreatedOn { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public string ServiceCode { get; set; }
    public OrderState State { get; set; }
    public OrderType Type { get; set; }
    public long UserId { get; set; }
    public PaymentType PaymentType { get; set; }

    public string DiscountNumber { get; set; }

    public static string GenerateNewTrackingCode()
    {
        return "O" + DateTime.UtcNow.Ticks.ToString().Substring(6, 12);
    }
}
public enum OrderState : int
{
    [Display(Name = "در انتظار واریز..⌛️")] WaitingForPayment = 0,
    [Display(Name = "در انتظار تائید پرداخت توسط اپراتور..⌛")] WaitingForConfirmRecept = 1,
    [Display(Name = "انجام شده ✅️")] Done,
    [Display(Name = "رد شده ✖️️")] Declined
}

public enum OrderType : int
{
    [Display(Name = "ددیافت اشتراک جدید️")] New = 0,
    [Display(Name = "تمدید اشتراک️")] Extend
}
public enum PaymentType : int
{
    Cart = 0,
    Gateway,
    Crypto,
    Wallet
}