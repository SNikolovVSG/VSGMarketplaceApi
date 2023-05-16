using AutoMapper;
using FluentMigrator.Runner;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NLog;
using NLog.Web;
using System.Reflection;
using VSGMarketplaceApi.Data;
using VSGMarketplaceApi.Data.Extensions;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.Data.Repositories;
using VSGMarketplaceApi.Data.Repositories.Interfaces;
using VSGMarketplaceApi.Middleware;
using VSGMarketplaceApi.Profiles;
using VSGMarketplaceApi.Services;
using VSGMarketplaceApi.Services.Interfaces;
using VSGMarketplaceApi.Validators;


var logger = LogManager.Setup().LoadConfigurationFromAssemblyResource(Assembly.GetEntryAssembly(), "nlog.config").GetCurrentClassLogger();
try
{
    //Main TODO: Login
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddSingleton<DapperContext>();
    builder.Services.AddSingleton<Database>();

    builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(c => c.AddSqlServer2012()
                .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

    //builder.Services.AddLogging(c => c.AddFluentMigratorConsole())
    //    .AddFluentMigratorCore()
    //        .ConfigureRunner(c => c.AddSqlServer2012()
    //            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
    //            .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    //probvai vs scoped
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IItemRepository, ItemRepository>();
    builder.Services.AddScoped<IImageRepository, ImageRepository>();

    builder.Services.AddScoped<IValidator<Item>, ItemValidator>();
    builder.Services.AddScoped<IValidator<Order>, OrderValidator>();

    builder.Services.AddScoped<IOrdersService, OrdersService>();
    builder.Services.AddScoped<IItemsService, ItemsService>();

    //JWT
    //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //    .AddJwtBearer(options =>
    //    {
    //        options.RequireHttpsMetadata = true;
    //        options.SaveToken = true;
    //        options.TokenValidationParameters = new TokenValidationParameters()
    //        {
    //            ValidateIssuer = true,
    //            ValidateAudience = true,
    //            ValidateLifetime = true,
    //            ValidateIssuerSigningKey = true,
    //            ValidIssuer = builder.Configuration["Jwt:Issuer"],
    //            ValidAudience = builder.Configuration["Jwt:Audience"],
    //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    //        };
    //    });

    //Microsoft login
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
     .AddJwtBearer(options =>
     {
         options.Authority = builder.Configuration["AzureSettings:Authority"];
         options.Audience = builder.Configuration["AzureSettings:Client"];
     });

    //CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MyCorsPolicy", builder => builder
        //.WithOrigins("http://localhost:5500/")
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithHeaders("Accept", "Content-Type", "Origin", "X-My-Header"));
    });

    builder.Services.AddAuthorization();

    //JSON add
    builder.Services.AddControllers().AddNewtonsoftJson();

    //auto mapper
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    //Caching
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("MyCorsPolicy");

    app.UseHttpsRedirection();

    app.UseAuthentication();

    //app.UseRouting();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, ex.Message);
    //throw ex;
}
finally
{
    LogManager.Shutdown();
}