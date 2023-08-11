/*********************** RUN INTEGRATION TEST PREREQUISITE ********************************
 * cd ./dockers/compose
 * docker compose up -d
 * see more on the ReadMe file
 ******************************************************************************************/

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation

using EventSourcing.Backbone;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Skeleton.Abstractions;
using StackExchange.Redis;
using Xunit.Abstractions;


// cd ./dockers/compose
// docker compose up -d

namespace Skeleton.Service.IntegrationTest;

[Trait("test-type", "integration")]
public sealed class VersionAwareRedisTests : IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProductCycleVersionAwareConsumer _subscriber = A.Fake<IProductCycleVersionAwareConsumer>();

    private readonly string URI = $"integration-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-version-aware";

    private readonly ILogger _fakeLogger = A.Fake<ILogger>();

    private readonly string ENV = $"test";
    private const int TIMEOUT = 1000 * 20;

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionAwareRedisTests" /> class.
    /// </summary>
    /// <param name="outputHelper">The output helper.</param>
    public VersionAwareRedisTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;


        A.CallTo(() => _subscriber.IdeaAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .ReturnsLazily((ConsumerContext meta, string title, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Signature.Operation}, {title}, {desc}");
                return ValueTask.CompletedTask;
            });
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerContext>.Ignored, A<string>.Ignored, A<Version>.Ignored, A<string>.Ignored))
            .ReturnsLazily((ConsumerContext meta, string title, Version version, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Signature.Operation}, {title}, {version}, {desc}");
                return ValueTask.CompletedTask;
            });

        #region  A.CallTo(() => _fakeLogger...)

        A.CallTo(() => _fakeLogger.Log<string>(
            A<LogLevel>.Ignored,
            A<EventId>.Ignored,
            A<string>.Ignored,
            A<Exception>.Ignored,
            A<Func<string, Exception?, string>>.Ignored
            ))
            .Invokes<object, LogLevel, EventId, string, Exception, Func<string, Exception, string>>(
                (level, id, msg, ex, fn) =>
                   _outputHelper.WriteLine($"Info: {fn(msg, ex)}"));

        #endregion //  A.CallTo(() => _fakeLogger...)
    }

    #endregion // Ctor

    #region VersionAware_Test

    [Fact(Timeout = TIMEOUT)]
    public async Task VersionAware_Test()
    {
        _outputHelper.WriteLine("Don't forget to start the docker compose environment");
        _outputHelper.WriteLine(@"  cd ./dockers/compose");
        _outputHelper.WriteLine(@"  compose up -d");

        IProductCycleVersionAwareProducer producer = RedisProducerBuilder.Create()
                                        .Environment(ENV)
                                        .Uri(URI)
                                        .BuildProductCycleVersionAwareProducer();

        await producer.InitAsync("make a thing", "bla bla");
        var version = new Version(1, 1, 1);
        await producer.PlanedAsync("001", version, "bla...bla...");

        var cts = new CancellationTokenSource();
        IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                        .WithOptions(o => o with { MaxMessages = 2 })
                        .WithCancellation(cts.Token)
                        .Environment(ENV)
                        .Uri(URI)
                        .SubscribeProductCycleVersionAwareConsumer(_subscriber);

        await subscription.Completion;

        // validation
        A.CallTo(() => _subscriber.IdeaAsync(
                            A<ConsumerContext>.That.Matches(
                                        m => m.Metadata.Signature.Operation == nameof(IProductCycleVersionAwareProducer.InitAsync)),
                            "make a thing",
                            "bla bla"))
                        .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerContext>.That.Matches(
                                        m => m.Metadata.Signature.Operation == nameof(IProductCycleVersionAwareProducer.PlanedAsync)),
                            "001",
                            version,
                            "bla...bla..."))
                        .MustHaveHappenedOnceExactly();
    }

    #endregion // VersionAware_Test

    #region Dispose pattern

    ~VersionAwareRedisTests()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        string key = URI;
        IConnectionMultiplexer conn = RedisClientFactory.CreateProviderAsync(
                                                logger: _fakeLogger,
                                                configurationHook: cfg => cfg.AllowAdmin = true).Result;
        IDatabaseAsync db = conn.GetDatabase();

        db.KeyDeleteAsync(key, CommandFlags.DemandMaster).Wait();
    }

    #endregion // Dispose pattern
}
