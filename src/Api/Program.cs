using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//var isAspire = builder
//    .Configuration.GetSection("Aspire")
//    .GetValue("UseAspire", false);

var useAspireRedis = builder
    .Configuration.GetSection("Aspire")
    .GetValue("UseRedis", false);

var useAspirePostgres = builder
    .Configuration.GetSection("Aspire")
    .GetValue("UsePostgres", false);

ConfigureExternalServices(builder);

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
void ConfigureExternalServices(WebApplicationBuilder builder)
{
    builder.Services.AddProblemDetails();

    if (useAspireRedis)
    {
        builder.AddRedisClient("redis");
    }
    else
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(o =>
        {
            var redis = builder.Configuration.GetConnectionString("redis");
            ArgumentNullException.ThrowIfNull(redis);
            return ConnectionMultiplexer.Connect(redis);
        });
    }

    if (useAspirePostgres)
    {
        builder.AddNpgsqlDbContext<ApiDbContext>("postgres");
    }
    else
    {
        builder.Services.AddDbContext<ApiDbContext>(o =>
        {
            var conn = builder.Configuration.GetConnectionString("postgres");
            o.UseNpgsql(conn, o => o.MigrationsAssembly("DbEf.Postgresql"));
        });
    }
}
void ConfigureServices(
    IServiceCollection services,
    IWebHostEnvironment env,
    ConfigurationManager config
)
{
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            if (env.IsDevelopment())
            {
                builder.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost"
                );
            }
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    services.AddRouting(o => o.LowercaseUrls = true);
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    if (env.IsDevelopment())
    {
        services.AddHttpLogging(o => { });
        services.AddOpenApiDocument(o =>
        {
            o.Title = "Muzonia API";
        });
    }
}
