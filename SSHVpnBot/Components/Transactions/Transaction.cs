namespace SSHVpnBot.Components.Transactions;

public class Transaction
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public TransactionType Type { get; set; }
    public string TransactionCode { get; set; }
    public bool IsRemoved { get; set; }

    public static string GenerateNewDiscountNumber()
    {
        return "T" +       
               DateTime.UtcNow.Ticks.ToString().Substring(6, 12);

    }
}

public enum TransactionType : int
{
    Deposit = 0,
    Withdrawl,
    Order,
    OrderReward,
    Charge,
    CashBack,
    StartViaReferral,
    Checkout,
    Apology
}