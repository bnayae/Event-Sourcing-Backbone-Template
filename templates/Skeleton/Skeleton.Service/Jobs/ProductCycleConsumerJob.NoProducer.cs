#pragma warning disable HAA0101 // Array allocation for params parameter

using Skeleton.Abstractions;

using EventSourcing.Backbone;
using EventSourcing.Backbone.Building;

namespace Skeleton.Controllers;

/// <summary>
/// Consumer job
/// </summary>
/// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
/// <seealso cref="System.IAsyncDisposable" />
/// <seealso cref="System.IDisposable" />
public sealed class ConsumerJob : IHostedService, IProductCycleConsumer
{
    private readonly IConsumerSubscribeBuilder _builder;
    private CancellationTokenSource? _cancellationTokenSource;
    private IConsumerLifetime? _subscription;

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="consumerBuilderKeyed">The consumer builder.</param>
    public ConsumerJob(
        ILogger<ConsumerJob> logger,
        IKeyed<IConsumerReadyBuilder> consumerBuilderKeyed)
    {
        if (!consumerBuilderKeyed.TryGet(ProductCycleConstants.URI, out var consumerBuilder)) 
            throw new EventSourcingException($"Consumer's registration found under the [{ProductCycleConstants.URI}] key.");
        _builder = consumerBuilder.WithLogger(logger);
        _logger = logger;
        logger.LogInformation("Consumer starts listening on: {URI}", ProductCycleConstants.URI);
    }

    /// <summary>
    /// Start Consumer Job.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var canellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        _subscription = _builder
                                .Group(ProductCycleConstants.CONSUMER_GROUP)
                                .WithCancellation(canellation.Token)
                                // this extension is generate (if you change the interface use the correlated new generated extension method)
                                .SubscribeProductCycleConsumer(this);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the Consumer Job.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.CancelSafe();
        await (_subscription?.Completion ?? Task.CompletedTask);
    }

    async ValueTask IProductCycleConsumer.IdeaAsync(ConsumerContext ctx, string title, string describe)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, "handling {event} [{id}]: {title}", meta.Signature.Operation, meta.MessageId, title);
        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.PlanedAsync(ConsumerContext ctx, string id, Version version, string doc)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {doc}
                                   """, meta.Signature.Operation, id, version, doc);

        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.ReviewedAsync(ConsumerContext ctx, string id, Version version, string[] notes)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {notes}
                                   """, meta.Signature.Operation, id, version, string.Join("\r\n- ", notes));

        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.ImplementedAsync(ConsumerContext ctx, string id, Version version)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   """, meta.Signature.Operation, id, version);
        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.TestedAsync(ConsumerContext ctx, string id, Version version, string[] notes)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {notes}
                                   """, meta.Signature.Operation, id, version, string.Join("\r\n- ", notes));

        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.DeployedAsync(ConsumerContext ctx, string id, Version version)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, "handling {event} [{id}]: {version}", meta.Signature.Operation, id, version);

        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.RejectedAsync(ConsumerContext ctx,
                                                        string id,
                                                        System.Version version,
                                                        string operation,
                                                        NextStage nextStage,
                                                        string[] notes)
    {
        var meta = ctx.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   operation = {operation}

                                   {notes}
                                   ---
                                   """, meta.Signature.Operation, id, version, operation, string.Join("\r\n- ", notes));

        await ctx.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }
}

