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

    #region Ctor

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

    #endregion Ctor

    #region OnStartAsync

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

    #endregion // OnStartAsync

    #region StopAsync

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

    #endregion // StopAsync

    // TODO: enrich telemetry

    async ValueTask IProductCycleConsumer.IdeaAsync(ConsumerMetadata consumerMetadata, string title, string describe)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, "handling {event} [{id}]: {title}", meta.Operation, meta.MessageId, title);
        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.PlanedAsync(ConsumerMetadata consumerMetadata, string id, Version version, string doc)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {doc}
                                   """, meta.Operation, id, version, doc);

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.ReviewedAsync(ConsumerMetadata consumerMetadata, string id, Version version, string[] notes)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {notes}
                                   """, meta.Operation, id, version, string.Join("\r\n- ", notes));

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.ImplementedAsync(ConsumerMetadata consumerMetadata, string id, Version version)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   """, meta.Operation, id, version);
        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.TestedAsync(ConsumerMetadata consumerMetadata, string id, Version version, string[] notes)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   {notes}
                                   """, meta.Operation, id, version, string.Join("\r\n- ", notes));

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.DeployedAsync(ConsumerMetadata consumerMetadata, string id, Version version)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, "handling {event} [{id}]: {version}", meta.Operation, id, version);

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.RejectedAsync(ConsumerMetadata consumerMetadata,
                                                        string id,
                                                        System.Version version,
                                                        string operation,
                                                        NextStage nextStage,
                                                        string[] notes)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   operation = {operation}

                                   {notes}
                                   ---
                                   """, meta.Operation, id, version, operation, string.Join("\r\n- ", notes));

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }
}

