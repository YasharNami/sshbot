using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.ServiceCategories.Repository;

public interface IServiceCategoryRepository : IBaseRepository<ServiceCategory>
{
    Task<List<ServiceCategory>> GetAllCategoriesAsync();
    Task<ServiceCategory?> GetByServiceCategoryCode(string code);
}