using EventSourcing.Backbone;
using Skeleton.Abstractions;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Skeleton;

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
    #if (s3)
    /// <param name="s3Bucket">The s3 bucket.</param>
    #endif
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        #if (s3)
        string s3Bucket,
        #endif
        Env env)
    {
        #if (s3)
        var s3Options = new S3Options { Bucket = s3Bucket };
        #endif
        services.AddSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
                                #if (s3) 
                                , s3Options 
                                #endif 
                                );
        });

        return services;
    }

    /// <summary>
    /// Register a producer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    #if (s3)
    /// <param name="s3Bucket">The s3 bucket.</param>
    #endif
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        #if (s3)
        string s3Bucket,
        #endif
        Env env)
    {
        #if (s3)
        var s3Options = new S3Options { Bucket = s3Bucket };
        #endif
        services.AddKeyedSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
                                    #if (s3)
                                    , s3Options
                                    #endif
            );
        }, uri);

        return services;
    }

    private static IProductCycleProducer BuildProducer(string uri, Env env, IServiceProvider ioc
    #if (s3)
    , S3Options s3Options
    #endif
    )
    {
        ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new EventSourcingException("Logger is missing");
        IProductCycleProducer producer = ioc.ResolveRedisProducerChannel()
                                #if (s3)
                                .ResolveS3Storage(s3Options)
                                #endif
                                .Environment(env)
                                .Uri(uri)
                                .WithLogger(logger)
                                .BuildProductCycleProducer();
        return producer;
    }
}
