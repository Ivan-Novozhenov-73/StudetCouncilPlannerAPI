using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.Dtos;
using StudetCouncilPlannerAPI.Services;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<List<EventListItemDto>>> GetEvents([FromQuery] EventListQueryDto query)
        {
            var events = await _eventService.GetEventsAsync(query);
            return Ok(events);
        }

        // GET: api/Event/actual
        [HttpGet("actual")]
        public async Task<ActionResult<List<EventListItemDto>>> GetActualEvents([FromQuery] EventListQueryDto query)
        {
            query.IsActual = true;
            var events = await _eventService.GetEventsAsync(query);
            return Ok(events);
        }

        // GET: api/Event/archive
        [HttpGet("archive")]
        public async Task<ActionResult<List<EventListItemDto>>> GetArchiveEvents([FromQuery] EventListQueryDto query)
        {
            query.IsActual = false;
            var events = await _eventService.GetEventsAsync(query);
            return Ok(events);
        }

        // GET: api/Event/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound();
            return Ok(ev);
        }

        // POST: api/Event
        [HttpPost]
        [Authorize(Roles = "1,2")] // только роли 1 и 2
        public async Task<ActionResult<Guid>> CreateEvent([FromBody] EventCreateDto dto)
        {
            // Здесь предполагается, что userId можно получить из JWT-токена
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
            if (userIdClaim == null) return Unauthorized();
            Guid userId = Guid.Parse(userIdClaim.Value);

            var eventId = await _eventService.CreateEventAsync(dto, userId);
            return CreatedAtAction(nameof(GetEvent), new { id = eventId }, eventId);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")] // только роли 1 и 2
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto dto)
        {
            var success = await _eventService.UpdateEventAsync(id, dto);
            if (!success)
                return NotFound();
            return NoContent();
        }

        // GET: api/Event/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<EventListItemDto>>> GetUserEvents(Guid userId)
        {
            var events = await _eventService.GetUserEventsAsync(userId);
            return Ok(events);
        }

        // POST: api/Event/{eventId}/participants/{userId}
        [HttpPost("{eventId}/participants/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddParticipant(Guid eventId, Guid userId)
        {
            var success = await _eventService.AddParticipantAsync(eventId, userId);
            if (!success)
                return BadRequest("User is already a participant.");
            return Ok();
        }

        // DELETE: api/Event/{eventId}/participants/{userId}
        [HttpDelete("{eventId}/participants/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemoveParticipant(Guid eventId, Guid userId)
        {
            var success = await _eventService.RemoveParticipantAsync(eventId, userId);
            if (!success)
                return NotFound();
            return Ok();
        }

        // POST: api/Event/{eventId}/organizers/{userId}
        [HttpPost("{eventId}/organizers/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddOrganizer(Guid eventId, Guid userId)
        {
            var success = await _eventService.AddOrganizerAsync(eventId, userId);
            if (!success)
                return BadRequest("User is already an organizer.");
            return Ok();
        }

        // DELETE: api/Event/{eventId}/organizers/{userId}
        [HttpDelete("{eventId}/organizers/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemoveOrganizer(Guid eventId, Guid userId)
        {
            var success = await _eventService.RemoveOrganizerAsync(eventId, userId);
            if (!success)
                return NotFound();
            return Ok();
        }

        // POST: api/Event/{eventId}/partners/{partnerId}
        [HttpPost("{eventId}/partners/{partnerId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddPartner(Guid eventId, Guid partnerId)
        {
            var success = await _eventService.AddPartnerAsync(eventId, partnerId);
            if (!success)
                return BadRequest("Partner is already attached.");
            return Ok();
        }

        // DELETE: api/Event/{eventId}/partners/{partnerId}
        [HttpDelete("{eventId}/partners/{partnerId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemovePartner(Guid eventId, Guid partnerId)
        {
            var success = await _eventService.RemovePartnerAsync(eventId, partnerId);
            if (!success)
                return NotFound();
            return Ok();
        }

        // GET: api/Event/{eventId}/organizers
        [HttpGet("{eventId}/organizers")]
        public async Task<ActionResult<List<UserShortDto>>> GetOrganizers(Guid eventId)
        {
            var organizers = await _eventService.GetOrganizersAsync(eventId);
            return Ok(organizers);
        }

        // GET: api/Event/{eventId}/participants
        [HttpGet("{eventId}/participants")]
        public async Task<ActionResult<List<UserShortDto>>> GetParticipants(Guid eventId)
        {
            var participants = await _eventService.GetParticipantsAsync(eventId);
            return Ok(participants);
        }

        // GET: api/Event/{eventId}/partners
        [HttpGet("{eventId}/partners")]
        public async Task<ActionResult<List<PartnerShortDto>>> GetPartners(Guid eventId)
        {
            var partners = await _eventService.GetPartnersAsync(eventId);
            return Ok(partners);
        }
    }
}