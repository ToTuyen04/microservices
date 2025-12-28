using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;
using eCommerce.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Infrastructure;

/// <summary>
/// Extension method to add infrastructure services to the dez
/// </summary>
/// <param name="services"></param>"
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        //TO DO: Add services to the IoC container
        //Infrasturcture services often include data access, catching and orther low-level components.

        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<DapperDbContext>();
        return services;

    } 
}

