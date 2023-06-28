using System.Threading.Channels;

using EventSourcing.Backbone;
using EventSourcing.Backbone.Building;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;
using Skeleton.Abstractions;
using System;

#pragma warning disable HAA0301 // Closure Allocation Source


namespace Skeleton.Service.UnitTests;

[Trait("test-type", "unit")]
public class EndToEndTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProductCycleConsumer _subscriber = A.Fake<IProductCycleConsumer>();
    private readonly Channel<Announcement> _channel =  Channel.CreateUnbounded<Announcement>();

    public EndToEndTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        A.CallTo(() => _subscriber.IdeaAsync(A<ConsumerMetadata>.Ignored, A<string>.Ignored, A<string>.Ignored))            
            .Invokes((ConsumerMetadata meta, string title, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Operation}, {title}, {desc}");
            });
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerMetadata>.Ignored, A<string>.Ignored, A<Version>.Ignored, A<string>.Ignored))
            .Invokes((ConsumerMetadata meta, string title, Version version, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Operation}, {title}, {version}, {desc}");
            });
    }

    [Fact]
    public async Task End2End_Test()
    {
        string uri = $"{ProductCycleConstants.URI}-test";

        IProductCycleProducer producer =
            ProducerBuilder.Empty.UseChannel(_ => new ProducerTestChannel(_channel))
                    .Uri(uri)
                    .BuildProductCycleProducer();

        await producer.IdeaAsync("make a thing", "bla bla");
        var version = new Version(1, 1, 1);
        await producer.PlanedAsync("001", version, "bla...bla...");

        var cts = new CancellationTokenSource();
        IAsyncDisposable subscription =
             ConsumerBuilder.Empty.UseChannel(_ => new ConsumerTestChannel(_channel))
                        .WithOptions(o =>  o with { MaxMessages = 2 })
                        .WithCancellation(cts.Token)
                        .Uri(uri)
                        .SubscribeProductCycleConsumer(_subscriber);

        await subscription.DisposeAsync();
        _channel.Writer.Complete();
        await _channel.Reader.Completion;

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
}
