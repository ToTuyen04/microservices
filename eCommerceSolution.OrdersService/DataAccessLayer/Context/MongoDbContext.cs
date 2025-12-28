using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Context;
public class MongoDbContext : DbContext
{
    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options) { }

    public DbSet<Orders> Orders { get; init; }
    public DbSet<OrderItem> OrderItems { get; init; }

    //Dùng trong trường hợp Unit Test, Console App, Background job,...
    //public static MongoDbContext Create(IMongoDatabase database) =>
    //    new(new DbContextOptionsBuilder<MongoDbContext>()
    //        .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
    //        .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Orders>().ToCollection("orders");
    }
}
