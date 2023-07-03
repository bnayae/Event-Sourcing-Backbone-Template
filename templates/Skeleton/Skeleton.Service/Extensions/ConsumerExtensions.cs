using EventSourcing.Backbone;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732

namespace Skeleton;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ConsumerExtensions
{
    /// <summary>
    /// Register a consumer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI.</param>
    #if (s3)
    /// <param name="s3Bucket">The s3 bucket.</param>
    #endif
    /// <returns></returns>
    public static WebApplicationBuilder AddConsumer (
                    this WebApplicationBuilder builder,
                    string uri
                    #if (s3)
                    , string s3Bucket
                    #endif
                    )
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        #if (s3)
        var s3Options = new S3Options { Bucket = s3Bucket };
        #endif
        services.AddSingleton(ioc =>
        {
            return BuildConsumer(uri, env, ioc 
            #if (s3) 
            , s3Options 
            #endif
            );
        });

        return builder;
    }

    /// <summary>
    /// Register a consumer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI of the stream (which is also used as the DI key).</param>
    #if (s3)
    /// <param name="s3Bucket">The s3 bucket.</param>
    #endif
    /// <returns></returns>
    public static WebApplicationBuilder AddKeyedConsumer (
                    this WebApplicationBuilder builder,
                    string uri
                    #if (s3)
                    , string s3Bucket
                    #endif
                    )
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        #if (s3)
        var s3Options = new S3Options { Bucket = s3Bucket };
        #endif
        services.AddKeyedSingleton(ioc =>
        {
            return BuildConsumer(uri
                                , env
                                , ioc
                                #if (s3)
                                , s3Options
                                #endif
                                );
        }, uri);

        return builder;
    }

    /// <summary>
    /// Builds the consumer.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="env">The environment.</param>
    /// <param name="ioc">The DI provider.</param>
    #if (s3)
    /// <param name="s3Options">The s3 options.</param>
    #endif
    /// <returns></returns>
    private static IConsumerReadyBuilder BuildConsumer(string uri
                                                        , Env env, IServiceProvider ioc
                                                        #if (s3)
                                                        , S3Options s3Options
                                                        #endif
                                                        )
    {
        return ioc.ResolveRedisConsumerChannel()
                        #if (s3)
                        .ResolveS3Storage(s3Options)
                        #endif
                        .WithOptions(o => o with
                        {
                            OriginFilter = MessageOrigin.Original,
                            AckBehavior = AckBehavior.OnSucceed,
                            // Expose debug leve telemetry
                            // TelemetryLevel = LogLevel.Debug 
                        })
                        .Environment(env)
                        .Uri(uri);
    }
}
