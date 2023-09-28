using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Colleagues.Repository;

public interface IOfferRulesRepository : IBaseRepository<OfferRule>
{
    Task<OfferRule> GetByServiceCode(string serviceCode);
}