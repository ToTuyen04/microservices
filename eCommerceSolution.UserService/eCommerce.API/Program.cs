using eCommerce.API.Middlewares;
using eCommerce.Core;
using eCommerce.Core.Mappers;
using eCommerce.Infrastructure;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Add Infrastructure services
builder.Services.AddInfrastructure();
builder.Services.AddCore();

//Add the controllers to the service collection
builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//Automapper (v.13+)
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ApplicationUserMappingProfile>();
    cfg.AddProfile<ApplicationUserToUserDTOMappingProfile>();
});

//Fluent Validation
builder.Services.AddFluentValidationAutoValidation();

// API explorer services
builder.Services.AddEndpointsApiExplorer();

//Add Swagger generation services
builder.Services.AddSwaggerGen();

//Add cors services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

//build the web application
var app = builder.Build();

//Middleware
app.UseExceptionHandlingMiddleware();

//Routing
app.UseRouting();
app.UseSwagger(); // add endpoint that can serve the swagger.json
app.UseSwaggerUI(); // add swagger ui to view the API docs
app.UseCors();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//Controller routes
app.MapControllers();

app.Run();
