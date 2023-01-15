using EventStoreAPI.Repositories;
using EGD.Command.Models;
using EGD.Command.Models.Enums;
using Microsoft.AspNetCore.Mvc;

//TODO:  change skip and limit to use a request object instead of passing it in the route.
//TODO:  need to finish adding paging, response needs to include totals.  Need a new response object which includes the List<CommandEventStorePublic>, TotalCount, Skip, Take
//TODO:  add gRPC duplex endpoint to push events as they are processed.

namespace EventStoreAPI.Controllers
{
    [ApiController]
    [Route("eventstore/")]
    public class EventStoreController : ControllerBase
    {
        private readonly ILogger<EventStoreController> _logger;
        private readonly ICommandEventRepository _repo;

        public EventStoreController(ILogger<EventStoreController> logger, ICommandEventRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }
        [HttpGet]
        [Route("commandevent/correlationid/{correlationId}/skip/{skip}/limit/{limit}/raw")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventsByCorrelationId(Guid? correlationId, int? limit = 10, int? skip =0)
        {
            var results = await _repo.GetCommandEventsAsync(correlationId, limit, skip);
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("commandevent/correlationid/{correlationId}/raw")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>),200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventsByCorrelationId(Guid? correlationId)
        {
            var results = await _repo.GetCommandEventsAsync(correlationId, 10, null);
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet]
        [Route("commandevent/eventaction/{eventaction}/raw")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventsByAction(string eventaction)
        {
            var results = await _repo.GetCommandEventsAsync(eventaction, 10, null);
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("commandevent/eventaction/{eventaction}/correlationid/{correlationId}/raw")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventsByAction(string eventaction, Guid? correlationId)
        {
            var results = await _repo.GetCommandEventsAsync(eventaction, correlationId, null, 10, null);
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("commandevent/eventaction/{eventaction}/correlationid/{correlationId}/commandstatus/{status}/raw")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventsByAction(string eventaction, Guid? correlationId, string status)
        {
            CommandStatus commandStatus;
            List<CommandEventStorePublic> results = null;
            if (Enum.TryParse(status, true, out commandStatus))
            {
                results = await _repo.GetCommandEventsAsync(eventaction, correlationId, commandStatus, 10, null);
            }
            else
            {
                results = await _repo.GetCommandEventsAsync(eventaction, correlationId, null, 10, null);
            }
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("commandevent/correlationid/{correlationId}/summary")]
        [ProducesResponseType(typeof(List<CommandEventStorePublic>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommandEventSummaryByCorrelationId(Guid? correlationId)
        {
            List<CommandEventSummary> results = null;
            if (correlationId.HasValue)
            {
                results = await _repo.GetSummary(correlationId.Value);
            }
            if (results != null && results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound();
            }
        }
    }
}