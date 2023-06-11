namespace Skeleton.Abstractions;

/// <summary>
/// Common constants
/// </summary>
public static class ProductCycleConstants
{
    public const string URI = "{CHANGE_THE_URI}";
    #if (EnableConsumer)
    public const string CONSUMER_GROUP = "{CHANGE_THE_CONSUMER_GROUP}";
    #endif
    #if (s3)
    public const string S3_BUCKET = "{CHANGE_THE_BUCKET}";
    #endif
}
