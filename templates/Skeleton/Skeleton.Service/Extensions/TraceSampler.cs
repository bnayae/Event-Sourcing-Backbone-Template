using OpenTelemetry.Trace;

//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://opentelemetry.io/docs/demo/services/cart/

namespace Skeleton;

/// <summary>
/// THIS IS A SAMPLE OF TRACING SAMPLER, PUT A THOUGHT IN IT TO AVOID UNEXPECTED DROPING
/// </summary>
/// <seealso cref="OpenTelemetry.Trace.Sampler" />
internal class TraceSampler : Sampler
{
    private const int SAMPLE_RATE = 4;

    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        int hash = samplingParameters.TraceId.GetHashCode();
        if ((hash % SAMPLE_RATE) == 0)
            return new SamplingResult(SamplingDecision.RecordAndSample);
        return new SamplingResult(SamplingDecision.Drop);
    }
}
