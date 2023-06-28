using System.Collections.Immutable;
using System.Threading.Channels;

using EventSourcing.Backbone;

namespace Skeleton.Service.UnitTests;

/// <summary>
/// In-Memory Channel (excellent for testing)
/// </summary>
/// <seealso cref="EventSourcing.Backbone.IProducerChannelProvider" />
public class ProducerTestChannel :
                        IProducerChannelProvider
{
    private readonly Channel<Announcement> _channel;
    private int _index;

    #region Ctor

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public ProducerTestChannel(Channel<Announcement> channel)
    {
        _channel = channel;
    }

    #endregion // Ctor

    #region SendAsync

    public async ValueTask<string> SendAsync(
        Announcement payload,
        ImmutableArray<IProducerStorageStrategyWithFilter> storageStrategy)
    {
        foreach (var strategy in storageStrategy)
        {
            await strategy.SaveBucketAsync(payload.Metadata.MessageId, payload.Segments, EventBucketCategories.Segments, payload.Metadata);
            await strategy.SaveBucketAsync(payload.Metadata.MessageId, payload.InterceptorsData, EventBucketCategories.Interceptions, payload.Metadata);
        }
        await _channel.Writer.WriteAsync(payload);
        return Interlocked.Increment(ref _index).ToString();
    }

    #endregion // SendAsync
}
