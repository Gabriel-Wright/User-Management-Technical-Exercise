using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using UserManagement.Models;

namespace UserManagement.Data;

/// <summary>
/// Here I intentionally do not use separate repository classes for each entity.
/// This approach keeps the architecture light while still allowing separation of
/// concerns through services and DTOs at higher layers.
///
/// - Users: full CRUD operations through IDataContext
/// - Audits: append-only logging of user operations; updates/deletes are not expected
///
/// For larger or more complex applications, dedicated repositories would be introduced, but for this
/// exercise and scope, decided to keep the single DataContext with generic methods.
/// </summary>
public class DataContext : DbContext, IDataContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
        if (Database.IsInMemory())
        {
            Database.EnsureCreated();
            DevelopmentDataSeeder.SeedUsers(this);
        }
    }

    protected DataContext(DbContextOptions options, bool skipSeeding)
        : base(options)
    {
        //skips EnsureCreated + SeedUsers
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuditEntity>()
    .HasOne(a => a.UserEntity)
    .WithMany()
    .HasForeignKey(a => a.UserEntityId);

        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.Deleted);
    }
    public DbSet<UserEntity>? Users { get; set; }

    public DbSet<UserAuditEntity>? Audits { get; set; }
    public DbSet<UserAuditChangeEntity>? AuditChanges { get; set; }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();

    public async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class
    => await AddAsync(entity);
    public void UpdateE<TEntity>(TEntity entity) where TEntity : class
        => Set<TEntity>().Update(entity);

    public void Delete<TEntity>(TEntity entity) where TEntity : class
        => Set<TEntity>().Remove(entity);
    public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
}
