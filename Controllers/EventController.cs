using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.Dtos;
using StudetCouncilPlannerAPI.Services;
using System.Security.Claims;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class EventController(EventService eventService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<EventShortDto>>> GetEvents([FromQuery] EventListQueryDto query) => 
            Ok(await eventService.GetEventsAsync(query)); 
        
        [HttpGet("actual")]
        public async Task<ActionResult<List<EventShortDto>>> GetActualEvents([FromQuery] EventListQueryDto query)
        {
            query.IsActual = true;
            var events = await eventService.GetEventsAsync(query);
            return Ok(events);
        }

        [HttpGet("archive")]
        public async Task<ActionResult<List<EventShortDto>>> GetArchiveEvents([FromQuery] EventListQueryDto query)
        {
            query.IsActual = false;
            var events = await eventService.GetEventsAsync(query);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id)
        {
            var ev = await eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound();
            return Ok(ev);
        }

        [HttpPost]
        [Authorize(Roles = "1,2")] // только роли 1 и 2
        public async Task<ActionResult<Guid>> CreateEvent([FromBody] EventCreateDto dto)
        {
            // Здесь предполагается, что userId можно получить из JWT-токена
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            Guid userId = Guid.Parse(userIdClaim.Value);

            var eventId = await eventService.CreateEventAsync(dto, userId);
            return CreatedAtAction(nameof(GetEvent), new { id = eventId }, eventId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")] // только роли 1 и 2
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto dto)
        {
            var success = await eventService.UpdateEventAsync(id, dto);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<EventShortDto>>> GetUserEvents(Guid userId)
        {
            var events = await eventService.GetUserEventsAsync(userId);
            return Ok(events);
        }

        [HttpPost("{eventId}/participants/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddParticipant(Guid eventId, Guid userId)
        {
            var success = await eventService.AddParticipantAsync(eventId, userId);
            if (!success)
                return BadRequest("User is already a participant.");
            return Ok();
        }

        [HttpDelete("{eventId}/participants/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemoveParticipant(Guid eventId, Guid userId)
        {
            var success = await eventService.RemoveParticipantAsync(eventId, userId);
            if (!success)
                return NotFound();
            return Ok();
        }

        [HttpPost("{eventId}/organizers/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddOrganizer(Guid eventId, Guid userId)
        {
            var success = await eventService.AddOrganizerAsync(eventId, userId);
            if (!success)
                return BadRequest("User is already an organizer.");
            return Ok();
        }

        [HttpDelete("{eventId}/organizers/{userId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemoveOrganizer(Guid eventId, Guid userId)
        {
            var success = await eventService.RemoveOrganizerAsync(eventId, userId);
            if (!success)
                return NotFound();
            return Ok();
        }

        [HttpPost("{eventId}/partners/{partnerId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddPartner(Guid eventId, Guid partnerId)
        {
            var success = await eventService.AddPartnerAsync(eventId, partnerId);
            if (!success)
                return BadRequest("Partner is already attached.");
            return Ok();
        }

        [HttpDelete("{eventId}/partners/{partnerId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> RemovePartner(Guid eventId, Guid partnerId)
        {
            var success = await eventService.RemovePartnerAsync(eventId, partnerId);
            if (!success)
                return NotFound();
            return Ok();
        }

        [HttpGet("{eventId}/organizers")]
        public async Task<ActionResult<List<UserShortDto>>> GetOrganizers(Guid eventId)
        {
            var organizers = await eventService.GetOrganizersAsync(eventId);
            return Ok(organizers);
        }

        [HttpGet("{eventId}/participants")]
        public async Task<ActionResult<List<UserShortDto>>> GetParticipants(Guid eventId)
        {
            var participants = await eventService.GetParticipantsAsync(eventId);
            return Ok(participants);
        }

        [HttpGet("{eventId}/partners")]
        public async Task<ActionResult<List<EventPartnerShortDto>>> GetPartners(Guid eventId)
        {
            var partners = await eventService.GetPartnersAsync(eventId);
            return Ok(partners);
        }
    }
}