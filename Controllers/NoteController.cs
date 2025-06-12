using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;
using System.Security.Claims;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class NoteController(NoteService noteService) : ControllerBase
    {
        // Получить список заметок для мероприятия
        [HttpGet("event/{eventId}"), Authorize]
        public async Task<ActionResult<List<NoteShortDto>>> GetNotesByEvent(Guid eventId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var hasAccess = await noteService.IsOrganizerOrChiefAsync(eventId, userId.Value);
            if (!hasAccess)
                return Forbid("Недостаточно прав для просмотра заметок этого мероприятия.");

            var notes = await noteService.GetNotesByEventAsync(eventId);
            return Ok(notes);
        }

        // Получить заметку по id
        [HttpGet("{noteId}"), Authorize]
        public async Task<ActionResult<NoteDetailDto>> GetNoteById(Guid noteId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            // Получаем заметку чтобы узнать EventId
            var note = await noteService.GetNoteByIdAsync(noteId);
            if (note == null)
                return NotFound();

            var hasAccess = await noteService.IsOrganizerOrChiefAsync(note.EventId, userId.Value);
            if (!hasAccess)
                return Forbid("Недостаточно прав для просмотра этой заметки.");

            return Ok(note);
        }

        // Создать заметку (только организатор/главный организатор)
        [HttpPost, Authorize]
        public async Task<ActionResult<Guid>> CreateNote([FromBody] NoteCreateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var noteId = await noteService.CreateNoteAsync(dto, userId.Value);
            if (noteId == null)
                return Forbid("Недостаточно прав или ошибка создания.");

            return CreatedAtAction(nameof(GetNoteById), new { noteId = noteId.Value }, noteId.Value);
        }

        // Редактировать заметку (только организатор/главный организатор)
        [HttpPut("{noteId}"), Authorize]
        public async Task<IActionResult> UpdateNote(Guid noteId, [FromBody] NoteUpdateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var success = await noteService.UpdateNoteAsync(noteId, dto, userId.Value);
            if (!success)
                return Forbid("Недостаточно прав или заметка не найдена.");

            return NoContent();
        }

        // Вспомогательный метод для получения идентификатора пользователя из токена
        private Guid? GetUserIdFromToken()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return null;
            return userId;
        }
    }
}