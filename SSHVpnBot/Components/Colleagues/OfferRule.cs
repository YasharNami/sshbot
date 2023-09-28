namespace SSHVpnBot.Components.Colleagues;

public class OfferRule
{
    public int Id { get; set; }
    public string ServiceCode { get; set; }
    public int BasePrice { get; set; }
    public int LessThan5Order { get; set; }
    public int MoreThan5Order { get; set; }
    public int MoreThan15Order { get; set; }
}