using DbEf;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var isAspire = builder.Configuration.GetSection("UseAspire").Get<bool>();

ConfigureAspire(builder);

// Add services to the container.
ConfigureServices(builder.Services, builder.Environment, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
    app.UseOpenApi(options =>
    {
        options.Path = "/openapi/{documentName}.json";
    });

    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "Muzonia API");
    });
    
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
    app.MapGet("/apiui", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
return;
void ConfigureAspire(WebApplicationBuilder builder)
{
    if (!isAspire)
    {
        return;
    }

    builder.AddRedisClient("redis");
    builder.AddNpgsqlDbContext<CoreDbContext>("apidb");
    builder.Services.AddProblemDetails();

}
void ConfigureServices(IServiceCollection services, IWebHostEnvironment env, ConfigurationManager config)
{
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            if (env.IsDevelopment())
            {
                builder.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost");
            }
            builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
        });
    });
    
    services.AddRouting(o => o.LowercaseUrls = true);
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    if (!isAspire)
    {
        services.AddDbContext<CoreDbContext>(o =>
        {
            o.UseNpgsql(config.GetConnectionString("postgres"));
        });
        // services.AddStackExchangeRedisCache(o =>
        // {
        //     o.Configuration = config.GetConnectionString("redis");
        // });
        services.AddSingleton<IConnectionMultiplexer>(o =>
        {
            var redis = config.GetConnectionString("redis");
            ArgumentNullException.ThrowIfNull(redis);
            return ConnectionMultiplexer.Connect(redis);
        });
    }
    if (env.IsDevelopment())
    {
        services.AddHttpLogging(o => { });
        services.AddOpenApiDocument(o =>
        {
            o.Title = "Muzonia API";
        });
    }
}
