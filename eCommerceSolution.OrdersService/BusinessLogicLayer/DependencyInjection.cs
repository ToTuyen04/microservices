using BusinessLogicLayer.DTO;
using BusinessLogicLayer.Mapper;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BusinessLogicLayer;
public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddAutoMapper(typeof(OrderToOrderResponseMappingProfile).Assembly);
        services.AddScoped<IOrdersService, OrdersService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
        });

        services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();
        services.AddHostedService<RabbitMQProductNameUpdatedHostedService>();
        return services;
    }
}

