using Diiwo.Core.Domain.Interfaces;
using Diiwo.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Diiwo.Core.Extensions;

/// <summary>
/// Extension methods for configuring Diiwo.Core services
/// Universal extensions for registering core functionality
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add core Diiwo services with custom CurrentUserService implementation
    /// </summary>
    /// <typeparam name="TCurrentUserService">Implementation of ICurrentUserService</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDiiwoCore<TCurrentUserService>(this IServiceCollection services)
        where TCurrentUserService : class, ICurrentUserService
    {
        // Register current user service (required for AuditInterceptor)
        services.AddScoped<ICurrentUserService, TCurrentUserService>();
        
        // Register audit interceptor
        services.AddScoped<AuditInterceptor>();

        return services;
    }

    /// <summary>
    /// Add Diiwo.Core with pre-registered ICurrentUserService
    /// Use this when you've already registered ICurrentUserService elsewhere
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDiiwoCoreWithExistingUserService(this IServiceCollection services)
    {
        // Only register audit interceptor (assumes ICurrentUserService already registered)
        services.AddScoped<AuditInterceptor>();

        return services;
    }
}