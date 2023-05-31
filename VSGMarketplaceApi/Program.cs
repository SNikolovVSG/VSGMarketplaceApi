using AutoMapper;
using Data;
using Data.Extensions;
using Data.Models;
using Data.Repositories;
using Data.Repositories.Interfaces;
using FluentMigrator.Runner;
using FluentValidation;
using System.Reflection;
using Helpers.Middleware;
using Helpers.Profiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NLog;
using NLog.Web;
using Services;
using Services.Interfaces;
using Helpers.Validators;

//TODO: location, update when change code
var logger = LogManager.Setup().LoadConfigurationFromAssemblyResource(Assembly.GetEntryAssembly(), "nlog.config").GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSingleton<DapperContext>();
    builder.Services.AddSingleton<Database>();

    builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(c => c.AddSqlServer2012()
                .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(Assembly.Load(GetAssembly("Data"))).For.Migrations());

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IItemRepository, ItemRepository>();
    builder.Services.AddScoped<IImageRepository, ImageRepository>();

    builder.Services.AddScoped<IValidator<Item>, ItemValidator>();
    builder.Services.AddScoped<IValidator<Order>, OrderValidator>();

    builder.Services.AddScoped<IOrdersService, OrdersService>();
    builder.Services.AddScoped<IItemsService, ItemsService>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["AzureSettings:Client"];
        options.Authority = builder.Configuration["AzureSettings:Authority"];
    });

    builder.Services.AddAuthorization
        (options => { options.AddPolicy("AdminOnly", policy => policy.RequireClaim("groups", Constants.AdminGroup)); });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MyCorsPolicy", builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithHeaders("Accept", "Content-Type", "Origin", "X-My-Header"));
    });

    builder.Services.AddControllers().AddNewtonsoftJson();

    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddMemoryCache();

    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    });

    IMapper mapper = config.CreateMapper();
    builder.Services.AddSingleton(mapper);

    var app = builder.Build();

    app.MigrateDatabase();

    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("MyCorsPolicy");

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
}
finally
{
    LogManager.Shutdown();
}

AssemblyName GetAssembly(string assemblyName)
{
    var assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
    var assembly = assemblies.FirstOrDefault(x => x.Name.Contains(assemblyName));
    return assembly;
}