using Company.Placeholder.Abstractions;
using Company.Placeholder.Service.Entities;

using EventSourcing.Backbone;

using Microsoft.AspNetCore.Mvc;

namespace Company.Placeholder.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductCycleProducerController : ControllerBase
{
    private readonly ILogger<ProducerController> _logger;
    private readonly IProductCycleProducer _producer;

    public ProducerController(
        ILogger<ProducerController> logger,
        IKeyed<IProductCycleProducer> producerKeyed)
    {
        _logger = logger;
        if (!producerKeyed.TryGet(ProductCycleConstants.URI, out var producer)) 
            throw new EventSourcingException($"Producer's registration found under the [{ProductCycleConstants.URI}] key.");
        _producer = producer;
    }

    /// <summary>
    /// Post order state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("idea")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    //[AllowAnonymous]
    public async Task<string> IdeaAsync(Idea payload)
    {
        var (title, describe) = payload;
        _logger.LogDebug("Sending idea event");
        EventKey key = await _producer.IdeaAsync(title, describe);
        return key;
    }

    /// <summary>
    /// Post packing state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("plan")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> PlanAsync([FromBody] Plan payload)
    {
        var (id_, describe) = payload;
        var (id, version) = id_;
        _logger.LogDebug("Sending plan event");
        EventKey key = await _producer.PlanedAsync(id, version, describe);
        return key;
    }

    /// <summary>
    /// Post on-delivery state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("review")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> ReviewAsync([FromBody] Review payload)
    {
        var (id_, notes) = payload;
        var (id, version) = id_;

        _logger.LogDebug("Sending review event");
        EventKey key = await _producer.ReviewedAsync(id, version, notes);
        return key;
    }

    /// <summary>
    /// Post on-received state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("implement")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> ImplementAsync([FromBody] Id payload)
    {
        var (id, version) = payload;

        _logger.LogDebug("Sending implement event");
        EventKey key = await _producer.ImplementedAsync(id, version);
        return key;
    }

    /// <summary>
    /// Post on-received state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> TestAsync([FromBody] Test payload)
    {
        var (id_, notes) = payload;
        var (id, version) = id_;

        _logger.LogDebug("Sending test event");
        EventKey key = await _producer.TestedAsync(id, version, notes);
        return key;
    }

    /// <summary>
    /// Post on-received state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("Deploy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> DeployAsync([FromBody] Id payload)
    {
        var (id, version) = payload;

        _logger.LogDebug("Sending deploy event");
        EventKey key = await _producer.DeployedAsync(id, version);
        return key;
    }
}