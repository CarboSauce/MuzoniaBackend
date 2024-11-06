namespace Muzonia.Api.DepInjection;

internal static class Cors
{
    public static IServiceCollection AddCors(
        this IServiceCollection services,
        IConfiguration cfg
    ) =>
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                var allowLocalhost = cfg.GetValue("AllowLocalhost", false);
                if (allowLocalhost)
                {
                    policyBuilder.SetIsOriginAllowed(origin =>
                        new Uri(origin).Host == "localhost"
                    );
                }
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
}
