using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Data;

public interface IDataContext
{
    IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;
    Task CreateAsync<TEntity>(TEntity entity) where TEntity : class;
    void UpdateE<TEntity>(TEntity entity) where TEntity : class;
    void Delete<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync();
}
