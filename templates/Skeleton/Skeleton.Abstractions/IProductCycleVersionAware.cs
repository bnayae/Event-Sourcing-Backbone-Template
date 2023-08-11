#pragma warning disable S1133 // Deprecated code should be removed

using EventSourcing.Backbone;
using Microsoft.Extensions.Logging;

namespace Skeleton.Abstractions;

using Generated.ProductCycleVersionAware;


/// <summary>
/// Event's schema definition
/// Return type of each method should be  <see cref="System.Threading.Tasks.ValueTask"/>
/// </summary>
[EventsContract(EventsContractType.Producer, MinVersion = 1)] // for the migration demo, the producer version lags behind the consumer version by one step 
[EventsContract(EventsContractType.Consumer, MinVersion = 2)]
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
    [EventSourceDeprecate (EventsContractType.Consumer, Date =  "2023-08-21")]
    //[EventSourceDeprecateVersion(EventsContractType.Producer, Date =  "2023-08-21")]
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

    #region Fallback

    /// <summary>
    /// Consumers the fallback.
    /// Excellent for Migration scenario
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static async Task<bool> Fallback(IConsumerInterceptionContext ctx, IProductCycleVersionAwareConsumer target)
    {
        ILogger logger = ctx.Logger;
        ConsumerContext consumerContext = ctx.Context;
        Metadata meta = consumerContext.Metadata;
        if (ctx.IsMatchStartProjectAsync_V0_String_String_Deprecated())
        {
            var data = await ctx.GetStartProjectAsync_V0_String_String_DeprecatedAsync();
            await target.IdeaAsync(consumerContext, data.name, data.desc);
            await ctx.AckAsync();
            return true;
        }
        if (ctx.IsMatchInitAsync_V1_String_String_Deprecated())
        {
            var data = await ctx.GetInitAsync_V1_String_String_DeprecatedAsync();
            await target.IdeaAsync(consumerContext, data.key, data.describe);
            await ctx.AckAsync();
            return true;
        }
        if (ctx.IsMatchCommentAsync_V2_String_Version_String_Deprecated())
        {
            var data = await ctx.GetCommentAsync_V2_String_Version_String_DeprecatedAsync();
            await target.ReviewedAsync(consumerContext, data.id, data.version, data.comment);
            await ctx.AckAsync();
            return true;
        }

        logger.LogWarning("Fallback didn't handle: {uri}, {signature}", meta.Uri, meta.Signature);
        //await ctx.CancelAsync();
        return false;
    }

    #endregion // Fallback
}