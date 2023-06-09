using EventSourcing.Backbone;
using Company.Placeholder.Abstractions;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Company.Placeholder;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ProductCycleProducerExtensions
{
    /// <summary>
    /// Register a producer.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="s3Bucket">The s3 bucket.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        string s3Bucket,
        Env env)
    {
        var s3Options = new S3Options { Bucket = s3Bucket };
        services.AddSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc, s3Options);
        });

        return services;
    }

    /// <summary>
    /// Register a producer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="s3Bucket">The s3 bucket.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        string s3Bucket,
        Env env)
    {
        var s3Options = new S3Options { Bucket = s3Bucket };
        services.AddKeyedSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc, s3Options);
        }, uri);

        return services;
    }

    private static IProductCycleProducer BuildProducer(string uri, Env env, IServiceProvider ioc, S3Options s3Options)
    {
        ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new EventSourcingException("Logger is missing");
        IProductCycleProducer producer = ioc.ResolveRedisProducerChannel()
                               .ResolveS3Storage(s3Options)
                             .Environment(env)
                             .Uri(uri)
                             .WithLogger(logger)
                             .BuildProductCycleProducer();
        return producer;
    }
}
