namespace webdaga.DbContext;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webdaga.Areas.admin.Models;
using webdaga.Models;

public class ApplicationDbContext : IdentityDbContext<UserModel>
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }

        }
    }


    public DbSet<articlesModel> Articles { get; set; }
    public DbSet<ChatMessageModel> ChatMessages { get; set; }



}