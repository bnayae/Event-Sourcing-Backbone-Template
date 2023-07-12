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
    private readonly IProductCycleProducer _producer;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="consumerBuilderKeyed">The consumer builder.</param>
    /// <param name="producer">The producer.</param>
    public ConsumerJob(
        ILogger<ConsumerJob> logger,
        IKeyed<IConsumerReadyBuilder> consumerBuilderKeyed,
        IProductCycleProducer producer)
    {
        if (!consumerBuilderKeyed.TryGet(ProductCycleConstants.URI, out var consumerBuilder)) 
            throw new EventSourcingException($"Consumer's registration found under the [{ProductCycleConstants.URI}] key.");
        _builder = consumerBuilder.WithLogger(logger);
        _logger = logger;
        _producer = producer;
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

    async ValueTask IProductCycleConsumer.IdeaAsync(ConsumerMetadata consumerMetadata, string title, string describe)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                    handling {event} [{id}]: {title}
                                    Let's build a plan..... 
                                    """, meta.Operation, meta.MessageId, title);
        await Task.Delay(Environment.TickCount % 1000 + 100);

        await _producer.PlanedAsync(meta.MessageId, new Version(0, 0, 1, 0), "Just do it this way....");
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
                                   ---
                                   Now we'll review it than, either approve or reject the plan.
                                   """, meta.Operation, id, version, doc);

        int delay = Environment.TickCount % 2_000 + 500;
        await Task.Delay(delay);
        if (delay < 550)
            await _producer.RejectedAsync(id, version, meta.Operation, NextStage.Abandon, "Seems to complex");
        else if (delay < 600)
            await _producer.RejectedAsync(id, version, meta.Operation, NextStage.Reject, "Need some refinement about ...");
        else
            await _producer.ReviewedAsync(id, version, "Great plan");
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
                                   ---
                                   Let's implement it!
                                   """, meta.Operation, id, version, string.Join("\r\n- ", notes));

        int delay = Environment.TickCount % 5_000 + 1_500;
        await Task.Delay(delay);
        await _producer.ImplementedAsync(id, version);
        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.ImplementedAsync(ConsumerMetadata consumerMetadata, string id, Version version)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, """
                                   handling {event} [{id}]: {version}
                                   ---
                                   Now it's time for QA.
                                   """, meta.Operation, id, version);

        int delay = Environment.TickCount % 3_000 + 100;
        await Task.Delay(delay);
        if (delay < 600)
            await _producer.RejectedAsync(id, version, meta.Operation, NextStage.Reject, "Performance doesn't meet our SLA...");
        else
            await _producer.TestedAsync(id, version, "Ready for deployment");
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
                                   ---
                                   Start deploying
                                   """, meta.Operation, id, version, string.Join("\r\n- ", notes));

        int delay = Environment.TickCount % 2_000 + 100;
        await Task.Delay(delay);
        await _producer.DeployedAsync(id, version);

        if (delay < 1_500)
        {
            _logger.Log(level, "planing next iteration...");
            await _producer.DeployedAsync(id, version);
        }

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }

    async ValueTask IProductCycleConsumer.DeployedAsync(ConsumerMetadata consumerMetadata, string id, Version version)
    {
        var meta = consumerMetadata.Metadata;
        LogLevel level = meta.Environment == "prod" ? LogLevel.Debug : LogLevel.Information;
        _logger.Log(level, "handling {event} [{id}]: {version}", meta.Operation, id, version);
        int delay = Environment.TickCount % 2_000 + 100;
        if (delay > 600)
        {
            await Task.Delay(delay);
            _logger.Log(level, "planing next iteration...");
            await _producer.PlanedAsync(id, new Version(version.Major, version.Minor, version.Build, version.Revision + 1), "improving xyz...");
        }
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

        if (nextStage != NextStage.Abandon)
        {
            string message = operation switch
            {
                nameof(IProductCycleConsumer.ImplementedAsync) => "Fix bugs",
                nameof(IProductCycleConsumer.PlanedAsync) => "Improve plans",
                _ => string.Empty
            };
            _logger.Log(level, "Re-plan");
            await Task.Delay(1000);
            await _producer.PlanedAsync(id, version, message);
        }

        await consumerMetadata.AckAsync(); // not required on default setting or when configuring the consumer to Ack on success.
    }
}

