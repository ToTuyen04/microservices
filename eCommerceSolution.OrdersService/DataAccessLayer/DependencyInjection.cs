using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer;
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringTemplate = configuration.GetConnectionString("MongoDbUri")!;
        var connectionString = connectionStringTemplate
            .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
            .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
        services.AddScoped<IMongoDatabase>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
        });
        //services.AddDbContext<MongoDbContext>(options =>
        //{
        //    options.UseMongoDB(connectionString, "databaseName");
        //});
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        return services;
    }
}

