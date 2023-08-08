#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation

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
public sealed class EndToEndTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProductCycleConsumer _subscriber = A.Fake<IProductCycleConsumer>();
    private readonly Channel<Announcement> _channel =  Channel.CreateUnbounded<Announcement>();

    public EndToEndTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        A.CallTo(() => _subscriber.IdeaAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))            
            .Invokes((ConsumerContext meta, string title, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Signature.Operation}, {title}, {desc}");
            });
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerContext>.Ignored, A<string>.Ignored, A<Version>.Ignored, A<string>.Ignored))
            .Invokes((ConsumerContext meta, string title, Version version, string desc) =>
            {
                _outputHelper.WriteLine($"{meta.Metadata.Signature.Operation}, {title}, {version}, {desc}");
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
                            A<ConsumerContext>.That.Matches(
                                        m => m.Metadata.Signature.Operation == nameof(IProductCycleConsumer.IdeaAsync)),
                            "make a thing",
                            "bla bla"))
                        .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.PlanedAsync(
                            A<ConsumerContext>.That.Matches(
                                        m => m.Metadata.Signature.Operation == nameof(IProductCycleConsumer.PlanedAsync)),
                            "001",
                            version,
                            "bla...bla..."))
                        .MustHaveHappenedOnceExactly();
    }
}
