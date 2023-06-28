#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable S108 // Nested blocks of code should not be left empty
#pragma warning disable CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed

using System.Threading.Channels;

namespace EventSourcing.Backbone;


public class ConsumerTestChannel : IConsumerChannelProvider
{
    private readonly Channel<Announcement> _channel;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public ConsumerTestChannel(Channel<Announcement> channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// Subscribe to the channel for specific metadata.
    /// </summary>
    /// <param name="plan">The metadata.</param>
    /// <param name="func">The function.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// When completed
    /// </returns>
    public async ValueTask SubscribeAsync(
                IConsumerPlan plan,
                Func<Announcement, IAck, ValueTask<bool>> func,
                CancellationToken cancellationToken)
    {
        while (!_channel.Reader.Completion.IsCompleted &&
               !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var announcement = await _channel.Reader.ReadAsync(cancellationToken);
                foreach (var strategy in await plan.StorageStrategiesAsync)
                {
                    await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Segments, m => string.Empty);
                    await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Interceptions, m => string.Empty);
                }
                await func(announcement, Ack.Empty);
            }
            catch (ChannelClosedException) { }
        }
    }


    public ValueTask<Announcement> GetByIdAsync(EventKey entryId, IConsumerPlan plan, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<Announcement> GetAsyncEnumerable(IConsumerPlan plan, ConsumerAsyncEnumerableOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        yield break;
    }
}
