using CommandAPI.Middleware;
using CommandAPI.Models;
using CommandAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Azure.Storage.Blobs;
using EGD.Command.Models.Extensions;
using EGD.Command.Models.Enums;

namespace CommandAPI.Controllers
{
    [ApiController]
    [Route("command/")]
    public class PublisherController : ControllerBase
    {
        private readonly ILogger<PublisherController> _logger;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;
        private readonly IPublishMessageRepo _messageRepo;
        private readonly BlobContainerClient _blobClient;
        public PublisherController(ILogger<PublisherController> logger, ICorrelationIdGenerator correlationIdGenerator, IPublishMessageRepo messageRepo, BlobContainerClient blobClient)
        {
            _logger = logger;
            _correlationIdGenerator = correlationIdGenerator;
            _messageRepo = messageRepo;
            _blobClient = blobClient;
        }
        [HttpPost]
        [Route("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishRequest request)
        {
            try
            {
                Guid correlationId = Guid.Parse(_correlationIdGenerator.Get());
                Dictionary<string, object> userProperties = new Dictionary<string, object>();
                Message message = request.Payload.ConvertToMessage(CommandStatus.New, request.Action, correlationId, userProperties);
                if (message.Body.Length > 262144)
                {
                    return BadRequest("The request body cannot be greater than 262144.  Use command/publish/batch for large payloads.");
                }
                else
                {
                    await _messageRepo.PublishServiceBusMessageAsync(message);
                    return Accepted();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("publish/batch")]
        public async Task<IActionResult> PublishBatch([FromBody] PublishRequest request)
        {
            try
            {
                Guid correlationId = Guid.Parse(_correlationIdGenerator.Get());
                Dictionary<string, object> userProperties = new Dictionary<string, object>();
                Message message = await request.Payload.ConvertToLargeMessage(CommandStatus.New, request.Action, _blobClient, correlationId, userProperties);
                await _messageRepo.PublishServiceBusMessageAsync(message);
                return Accepted();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }

    
}