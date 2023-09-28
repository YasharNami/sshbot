using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.ServiceCategories.Repository;

public class ServiceCategoryRepository : BaseRepository<ServiceCategory>, IServiceCategoryRepository
{
    public async Task<List<ServiceCategory>> GetAllCategoriesAsync()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<ServiceCategory>($"select * from servicecategories where isremoved=0"))
                .ToList();
        }
    }

    public async Task<ServiceCategory?> GetByServiceCategoryCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<ServiceCategory>(
                $"select * from servicecategories where code=@code and isremoved=0",
                new { code });
        }
    }
}