using System.Text.Json;

using EventSourcing.Backbone;

using Microsoft.AspNetCore.Mvc;

namespace Company.Placeholder.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductCycleConsumerController : ControllerBase
{
    private readonly ILogger<ConsumerController> _logger;
    private readonly IConsumerReceiver _receiver;

    public ConsumerController(
        ILogger<ConsumerController> logger,
        IKeyed<IConsumerReadyBuilder> consumerBuilderKeyed)
    {
        _logger = logger;
        if (!consumerBuilderKeyed.TryGet(ProductCycleConstants.URI, out var consumerBuilder)) 
            throw new EventSourcingException($"The Consumer's registration found under the [{ProductCycleConstants.URI}] key.");
        _receiver = consumerBuilder.BuildReceiver();
    }

    /// <summary>
    /// Gets an event by event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns></returns>
    [HttpGet("{eventKey}")]
    public async Task<JsonElement> GetAsync(string eventKey)
    {
        _logger.LogDebug("fetching event [{key}]", eventKey);
        var json = await _receiver.GetJsonByIdAsync(eventKey);
        return json;
    }
}