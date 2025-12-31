using AutoMapper;
using BusinessLogicLayer.Mapper;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer;
public static class DependencyInjection
{
    public static IServiceCollection addBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            //phải khai báo đầy đủ các class MappingProfile
            cfg.AddProfile<ProductToProductResponseMappingProfile>();
            cfg.AddProfile<ProductAddRequestToProductMappingProfile>();
            cfg.AddProfile<ProductUpdateRequestToProductMappingProfile>();
        }, Assembly.GetExecutingAssembly());
        //services.AddAutoMapper(typeof(ProductToProductResponseMappingProfile).Assembly);

        //đăng ký toàn bộ các validator trong assembly này (vì các validator này đều đc kế thừa từ AbstractValidator<T>)
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
        services.AddScoped<IProductService, ProductService>();
        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();
        return services;
    }
}

