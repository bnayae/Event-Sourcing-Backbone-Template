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
public sealed class EndToEndRedisTests : IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProductCycleConsumer _subscriber = A.Fake<IProductCycleConsumer>();

    private readonly string URI = $"integration-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}";

    private readonly ILogger _fakeLogger = A.Fake<ILogger>();

    private readonly string ENV = $"test";
    private const int TIMEOUT = 1000 * 20;

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="EndToEndRedisTests" /> class.
    /// </summary>
    /// <param name="outputHelper">The output helper.</param>
    public EndToEndRedisTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;


        A.CallTo(() => _subscriber.IdeaAsync(A<ConsumerMetadata>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .ReturnsLazily((ConsumerMetadata meta, string title, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Operation}, {title}, {desc}");
                return ValueTask.CompletedTask;
            });
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerMetadata>.Ignored, A<string>.Ignored, A<Version>.Ignored, A<string>.Ignored))
            .ReturnsLazily((ConsumerMetadata meta, string title, Version version, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Operation}, {title}, {version}, {desc}");
                return ValueTask.CompletedTask;
            });

        #region  A.CallTo(() => _fakeLogger...)

        A.CallTo(() => _fakeLogger.Log<string>(
            A<LogLevel>.Ignored,
            A<EventId>.Ignored,
            A<string>.Ignored,
            A<Exception>.Ignored,
            A<Func<string, Exception, string>>.Ignored
            ))
            .Invokes<object, LogLevel, EventId, string, Exception, Func<string, Exception, string>>(
                (level, id, msg, ex, fn) =>
                   _outputHelper.WriteLine($"Info: {fn(msg, ex)}"));

        #endregion //  A.CallTo(() => _fakeLogger...)
    }

    #endregion // Ctor

    #region OnSucceed_ACK_Test

    [Fact(Timeout = TIMEOUT)]
    public async Task OnSucceed_ACK_Test()
    {
        _outputHelper.WriteLine("Don't forget to start the docker compose environment");
        _outputHelper.WriteLine(@"  cd ./dockers/compose");
        _outputHelper.WriteLine(@"  compose up -d");

        IProductCycleProducer producer = RedisProducerBuilder.Create()
                                        .Environment(ENV)
                                        .Uri(URI)
                                        .BuildProductCycleProducer();

        await producer.IdeaAsync("make a thing", "bla bla");
        var version = new Version(1, 1, 1);
        await producer.PlanedAsync("001", version, "bla...bla...");

        var cts = new CancellationTokenSource();
        IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                        .WithOptions(o => o with { MaxMessages = 2 })
                        .WithCancellation(cts.Token)
                        .Environment(ENV)
                        .Uri(URI)
                        .SubscribeProductCycleConsumer(_subscriber);

        await subscription.Completion;

        // validation
        A.CallTo(() => _subscriber.IdeaAsync(
                            A<ConsumerMetadata>.That.Matches(
                                        m => m.Metadata.Operation == nameof(IProductCycleConsumer.IdeaAsync)),
                            "make a thing",
                            "bla bla"))
                        .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerMetadata>.That.Matches(
                                        m => m.Metadata.Operation == nameof(IProductCycleConsumer.PlanedAsync)),
                            "001",
                            version,
                            "bla...bla..."))
                        .MustHaveHappenedOnceExactly();
    }

    #endregion // OnSucceed_ACK_Test

    #region Dispose pattern

    ~EndToEndRedisTests()
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
