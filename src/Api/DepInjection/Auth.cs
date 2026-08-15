using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Muzonia.Core.Common;
using ScottBrady.IdentityModel.Crypto;
using ScottBrady.IdentityModel.Tokens;

namespace Muzonia.Api.DepInjection;

internal static class Auth
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration config,
        ApiConfig apiConfig,
        IWebHostEnvironment env
    )
    {
        services.AddAuthorization();

        var authority =
            config["Auth:Authority"]
            ?? throw new InvalidOperationException(
                "Auth:Authority is required"
            );
        var audience =
            config["Auth:Audience"]
            ?? throw new InvalidOperationException("Auth:Audience is required");

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = false;
                    options.IncludeErrorDetails = true;
                    options.MapInboundClaims = false;
                    var jwks = RetrieveJwks(authority).ToArray();
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = authority,
                            ValidateAudience = true,
                            ValidAudience = audience,
                            ValidateLifetime = true,
                            IssuerSigningKeys = jwks,
                            ValidAudiences = [audience],
                            ClockSkew = TimeSpan.FromSeconds(30),
                        };
                }
            );

        IEnumerable<SecurityKey> RetrieveJwks(string url)
        {
            // https://github.com/dotnet/aspnetcore/issues/62416
            var jwksRaw = new HttpClient().GetStringAsync(url + "/jwks").Result;

            var jwks = JsonWebKeySet.Create(jwksRaw);

            var keys = new List<EdDsaSecurityKey>();
            foreach (
                var key in jwks.Keys.Where(k =>
                    k.Alg == "EdDSA" && k.Crv == "Ed25519"
                )
            )
            {
                // Decode the public key
                byte[] publicKeyBytes = Base64UrlEncoder.DecodeBytes(key.X);

                // Create EdDSA parameters with only the public key
                var parameters = new EdDsaParameters(
                    ExtendedSecurityAlgorithms.Curves.Ed25519
                )
                {
                    X = publicKeyBytes,
                };

                // Create EdDSA public key
                var edDsa = EdDsa.Create(parameters);

                keys.Add(new EdDsaSecurityKey(edDsa));
            }

            return keys;
        }
        return services;
    }
}
