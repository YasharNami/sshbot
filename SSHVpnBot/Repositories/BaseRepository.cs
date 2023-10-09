using System.Data.SqlClient;
using System.Reflection;
using ConnectBashBot.Telegram.Handlers;
using Dapper;

namespace SSHVpnBot.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity>
{
    public readonly string conString =
            $@"Server=65.21.154.21\MSSQLSERVER2019;Database={MainHandler._db};User Id=sa;Password=EqUxJ53xx3ULRAtNw7FH;TrustServerCertificate=true;";
    
    public TEntity GetById(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<TEntity>(
                $"select * from {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")} where Id=@id",
                new { id });
        }
    }

    public void Add(TEntity entity)
    {
        using (var db = new SqlConnection(conString))
        {
            var columns = GetColumns();
            var stringOfColumns = string.Join(", ", columns);
            var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
            var query =
                $"insert into {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")} ({stringOfColumns}) values ({stringOfParameters.Trim()})";

            db.Execute(query, entity);
        }
    }


    public void AddWithId(TEntity entity)
    {
        using (var db = new SqlConnection(conString))
        {
            var columns = GetColumnsWithId();
            var stringOfColumns = string.Join(", ", columns);
            var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
            var query =
                $"insert into {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")} ({stringOfColumns}) values ({stringOfParameters})";
            db.Execute(query, entity);
        }
    }

    public void Delete(TEntity entity)
    {
        using (var db = new SqlConnection(conString))
        {
            var query =
                $"delete from {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")}  where Id = @Id";
            db.Execute(query, entity);
        }
    }

    public void DeleteById(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            var query =
                $"delete from {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")}  where Id = {id}";
            db.Execute(query);
        }
    }

    public void LogicalDeleteById(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            var query =
                $"Update {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")}  set IsRemoved={0}  where Id = {id}";
            db.Execute(query);
        }
    }

    public void Update(TEntity entity)
    {
        using (var db = new SqlConnection(conString))
        {
            var columns = GetColumns().Where(c => c != null).ToList();
            var stringOfColumns = string.Join(", ", columns.Select(e => $"{e} = @{e}"));
            var query =
                $"update {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")}  set {stringOfColumns.Trim()} where Id = @Id";
            db.Execute(query, entity);
        }
    }


    public int Count()
    {
        using (var db = new SqlConnection(conString))
        {
            var query = $"select count(id) from users";
            return db.QuerySingleOrDefault<int>(query);
        }
    }

    public IEnumerable<string> GetColumns()
    {
        return typeof(TEntity)
            .GetProperties()
            .Where(e => e.Name.Trim() != "Id" && !e.PropertyType.GetTypeInfo().IsGenericType)
            .Select(e => e.Name.Trim());
    }

    public int DectiveCount()
    {
        var query = $"select count(id) from {typeof(TEntity).Name}s where isactive=0 and isremoved=0";
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<int>(query);
        }
    }

    public int ActiveCount()
    {
        var query = $"select count(id) from {typeof(TEntity).Name}s where isactive=1 and isremoved=0";
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<int>(query);
        }
    }

    public IEnumerable<string> GetColumnsWithId()
    {
        return typeof(TEntity)
            .GetProperties()
            .Where(e => !e.PropertyType.GetTypeInfo().IsGenericType)
            .Select(e => e.Name);
    }

    public IEnumerable<TEntity> GetAll()
    {
        using (var db = new SqlConnection(conString))
        {
            var query =
                $"select * from {(typeof(TEntity).Name.EndsWith("y") ? typeof(TEntity).Name.Substring(0, typeof(TEntity).Name.Length - 1) : typeof(TEntity).Name)}{(typeof(TEntity).Name.EndsWith('y') ? "ies" : "s")} ";
            return db.Query<TEntity>(query).ToList();
        }
    }
}