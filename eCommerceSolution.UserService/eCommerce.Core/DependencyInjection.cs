using eCommerce.Core.ServiceContracts;
using eCommerce.Core.Services;
using eCommerce.Core.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Core;

/// <summary>
/// Extension method to add infrastructure services to the dez
/// </summary>
/// <param name="services"></param>"
public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        //TO DO: Add services to the IoC container
        //Core services often include data access, catching and orther low-level components.
        services.AddTransient<IUsersService, UsersService>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;

    }
}

