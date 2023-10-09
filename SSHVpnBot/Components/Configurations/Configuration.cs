using System.ComponentModel.DataAnnotations;

namespace SSHVpnBot.Components.Configurations;


public class Configuration
{
    public int Id { get; set; }
    public string BankAccountOwner { get; set; }
    public string CardNumber { get; set; }
    public ConfigurationType Type { get; set; }
}

public enum ConfigurationType : int
{
    [Display(Name = "سمت مشتری")]
    CustomerSide = 0,
    [Display(Name = "سمت همکار")]
    CollleagueSide
}