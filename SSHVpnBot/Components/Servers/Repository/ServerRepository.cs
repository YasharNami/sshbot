using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Servers.Repository;

public class ServerRepository : BaseRepository<Server>, IServerRepository
{
    public async Task<Server?> GetServerByCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Server>($"select * from servers where code=@code and isremoved=0",
                new { code });
        }
    }

    public async Task<Server?> GetActiveOneByCategoryCode(int spapce, string category)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Server>(
                $"select top(1) * from servers where isactive=1  and isremoved=0 and categorycode=@category and type={(int)ServerType.Main} and capacity >= @spapce and capacity > 0 order by capacity desc",
                new
                    { spapce, category });
        }
    }

    public async Task<List<Server>> GetServersByCategoryCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Server>($"select * from servers where categorycode=@code and isremoved=0",
                new { code })).ToList();
        }
    }

    public async Task<List<Server>> GetAllByPaginationAsync(int page, int page_size)
    {
        using (var db = new SqlConnection(conString))
        {
            var result = (await db.QueryAsync<Server>(
                $"SELECT  locationcode,code,Capacity,Password,Url,UserName,IsActive, value domain FROM servers CROSS APPLY STRING_SPLIT(domain, '.') " +
                $" where IsRemoved=0 and value like 'cb%' order by cast(REPLACE(value,'cb','') as int) asc OFFSET " +
                $"{(page - 1) * page_size} ROWS FETCH NEXT {page_size} ROWS ONLY")).ToList();
            return result;
        }
    }

    public bool AnyByUrl(string serverCode, string url)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.Query($"select code from servers where code!=@code and url=@url  and isremoved=0",
                new { code = serverCode, url = url }).Any();
        }
    }

    public async Task<Server> GetActiveOne(int spaceNeeded)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Server>(
                $"select * from servers where isactive=1  and isremoved=0 and type={(int)ServerType.Main} and capacity >= @spaceNeeded order by capacity desc",
                new
                    { spaceNeeded });
        }
    }

    public async Task<Server> GetTestServer()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Server>(
                $"select * from servers where isactive=1 and capacity>=1 and isremoved=0 and type={(int)ServerType.Check}");
        }
    }

    public async Task<bool> Capacity(int neededCapacity, string category)
    {
        using (var db = new SqlConnection(conString))
        {
            var capacities = await db.QueryFirstOrDefaultAsync<int>(
                $"select sum(capacity) from servers where isactive=1 and categorycode='{category}' and isremoved=0 and capacity>0 and type={(int)ServerType.Main}");
            return capacities >= neededCapacity ? true : false;
        }
    }

    public bool AnyByDomain(string serverCode, string domain)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.Query($"select code from servers where code!=@code and domain=@domain  and isremoved=0",
                new { code = serverCode, domain = domain }).Any();
        }
    }

    public async Task<Server?> GetByDomainAsync(string domain)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Server>(
                $"select * from servers where domain=@domain and isremoved=0",
                new { domain });
        }
    }
    
    public async Task<List<Server>> GetColleagueServers()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Server>($"select * from servers where type=2 and isremoved=0")).ToList();
        }
    }
    
}