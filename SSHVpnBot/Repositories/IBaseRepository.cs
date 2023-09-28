namespace SSHVpnBot.Repositories;

public interface IBaseRepository<TEntity>
{
    void Add(TEntity entity);
    TEntity GetById(int id);
    int Count();
    void AddWithId(TEntity entity);
    void Delete(TEntity entity);
    void DeleteById(int id);
    int ActiveCount();
    int DectiveCount();
    void LogicalDeleteById(int id);
    void Update(TEntity entity);
    IEnumerable<string> GetColumns();
    IEnumerable<string> GetColumnsWithId();
    IEnumerable<TEntity> GetAll();
}