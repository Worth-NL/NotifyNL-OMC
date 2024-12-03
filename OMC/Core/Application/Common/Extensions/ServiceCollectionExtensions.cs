// © 2023, Worth Systems.

using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService{TService}(IServiceProvider)"/>
        public static TService GetRequiredService<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.BuildServiceProvider().GetRequiredService<TService>();
        }
    }
}