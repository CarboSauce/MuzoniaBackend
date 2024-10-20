using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigureServices(builder.Services, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseDeveloperExceptionPage();
    app.UseOpenApi(o =>
    {
        o.Path = "openapi/{documentName}.json";
    
    });
    app.MapScalarApiReference(o =>
    {
        o
           .WithTitle("Muzonia API")
           .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
    });

    app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
return;

static void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
{
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
