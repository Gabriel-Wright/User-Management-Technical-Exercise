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
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuditEntity>()
    .HasOne(a => a.UserEntity)
    .WithMany()
    .HasForeignKey(a => a.UserEntityId);

        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.Deleted);

        modelBuilder.Entity<UserEntity>().HasData(new[]
        {
        new UserEntity { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-25) },
        new UserEntity { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-40) },
        new UserEntity { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", IsActive = false, Deleted = false, BirthDate = DateTime.Today.AddYears(-35) },
        new UserEntity { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-50) },
        new UserEntity { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-29) },
        new UserEntity { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-60) },
        new UserEntity { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", IsActive = false, Deleted = false, BirthDate = DateTime.Today.AddYears(-33) },
        new UserEntity { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", IsActive = false, Deleted = false, BirthDate = DateTime.Today.AddYears(-28) },
        new UserEntity { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", IsActive = false, Deleted = false, BirthDate = DateTime.Today.AddYears(-45) },
        new UserEntity { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-38) },
        new UserEntity { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", IsActive = true, Deleted = false, BirthDate = DateTime.Today.AddYears(-32) }    });
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
