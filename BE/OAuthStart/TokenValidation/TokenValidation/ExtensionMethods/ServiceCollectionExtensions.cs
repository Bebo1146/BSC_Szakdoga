using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TokenValidation.TokenValidation.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds JWT bearer token validation using OIDC metadata from Authority.
        /// Reads config from section "Auth" by default.
        /// </summary>
        public static IServiceCollection AddTokenValidation(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "Auth")
        {
            // Bind + validate your custom options
            services.AddOptions<TokenValidationOptions>()
                .Bind(configuration.GetSection(sectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Authority), $"{sectionName}:Authority is required")
                // Audience is now optional for testing
                .ValidateOnStart();

            TokenValidationOptions? authOptions = configuration.GetSection(sectionName).Get<TokenValidationOptions>();

            // Register authentication scheme with direct configuration
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authOptions.Authority;
                        options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;

                        // Only set audience if provided
                        if (!string.IsNullOrWhiteSpace(authOptions.Audience))
                        {
                            options.Audience = authOptions.Audience;
                        }

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = authOptions.Authority,
                            ValidateAudience = !string.IsNullOrWhiteSpace(authOptions.Audience),
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ClockSkew = TimeSpan.FromSeconds(authOptions.ClockSkewSeconds),
                        };

                        // Add detailed event handlers for debugging
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                ILogger logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtAuthentication");

                                logger.LogError(context.Exception,
                                    "JWT Authentication Failed: {Message}",
                                    context.Exception.Message);

                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                ILogger logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtAuthentication");

                                logger.LogInformation("JWT Token Validated Successfully for user: {User}",
                                    context.Principal?.Identity?.Name ?? "Unknown");

                                return Task.CompletedTask;
                            },
                            OnChallenge = context =>
                            {
                                ILogger logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtAuthentication");

                                logger.LogWarning("JWT Challenge: {Error} - {ErrorDescription}",
                                    context.Error,
                                    context.ErrorDescription);

                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context =>
                            {
                                ILogger logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtAuthentication");

                                logger.LogDebug("JWT Token received from Authorization header");

                                return Task.CompletedTask;
                            }
                        };
                    });

            // Add authorization + optional scope policies
            services.AddAuthorization();
            services.AddTokenValidationPolicies();

            return services;
        }

        /// <summary>
        /// Adds authorization policies based on TokenValidationOptions.RequiredScopes.
        /// </summary>
        public static IServiceCollection AddTokenValidationPolicies(this IServiceCollection services)
        {
            // Build policy using configured options (captured values)
            services.AddOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions>()
                .Configure<IOptions<TokenValidationOptions>>((options, tvOpts) =>
                {
                    TokenValidationOptions opt = tvOpts.Value;

                    options.AddPolicy("RequireConfiguredScopes", policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        // If no required scopes configured, just require authentication
                        if (opt.RequiredScopes is null || opt.RequiredScopes.Length == 0)
                            return;

                        policy.RequireAssertion(context =>
                        {
                            string scopes =
                                context.User.FindFirst("scp")?.Value ??
                                context.User.FindFirst("scope")?.Value ??
                                "";

                            string[] scopeSet = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            // Requires ALL configured scopes. If you want "any", change All -> Any.
                            return opt.RequiredScopes.All(required => scopeSet.Contains(required));
                        });
                    });
                });

            return services;
        }
    }
}