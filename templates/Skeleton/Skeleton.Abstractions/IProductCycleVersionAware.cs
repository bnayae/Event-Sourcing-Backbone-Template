#pragma warning disable S1133 // Deprecated code should be removed

using EventSourcing.Backbone;

namespace Skeleton.Abstractions;

/// <summary>
/// Event's schema definition
/// Return type of each method should be  <see cref="System.Threading.Tasks.ValueTask"/>
/// </summary>
#if (EnableProducer)
[EventsContract(EventsContractType.Producer, MinVersion = 1)] // for the migration demo, the producer version lags behind the consumer version by one step 
#endif
#if (EnableConsumer)
[EventsContract(EventsContractType.Consumer, MinVersion = 2)]
#endif
[Obsolete("Choose either the Producer or Consumer version of this interface.")]
public interface IProductCycleVersionAware
{
    // version 0 by default
    ValueTask StartProjectAsync(string name, string desc);
    [EventSourceVersion(1, Date = "2023-08-01", Remark = "Example of a version-aware API (consider the minimum version: this will be accessible to the producer but marked as deprecated [hidden] for the consumer)")]
    ValueTask InitAsync(string key, string describe);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask IdeaAsync(string title, string describe);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask PlanedAsync(string id, Version version, string doc);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    #if (EnableConsumer)
    [EventSourceDeprecate(EventsContractType.Consumer, Date =  "2023-08-21")]
    #endif
    #if (EnableProducer)
    //[EventSourceDeprecate(EventsContractType.Producer, Date =  "2023-08-21")]
    #endif
    ValueTask CommentAsync(string id, Version version, string comment);

    [EventSourceVersion(3, Date = "2023-08-21", Remark = "Example of a version-aware API")]
    ValueTask ReviewedAsync(string id, Version version, params string[] notes);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask ImplementedAsync(string id, Version version);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask TestedAsync(string id, Version version, params string[] notes);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask DeployedAsync(string id, Version version);

    [EventSourceVersion(2, Date = "2023-08-16", Remark = "Example of a version-aware API")]
    ValueTask RejectedAsync(string id, Version version, string operation, NextStage nextStage, params string[] notes);
}