using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;
using System.Security.Claims;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class MeetingController(MeetingService meetingService) : ControllerBase
    {
        // Создать встречу (только глава или председатель студсовета)
        [HttpPost, Authorize]
        public async Task<ActionResult<Guid>> CreateMeeting([FromBody] MeetingCreateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var meetingId = await meetingService.CreateMeetingAsync(dto, userId.Value);
            if (meetingId == null)
                return Forbid("Недостаточно прав для создания встречи.");

            return CreatedAtAction(nameof(GetMeetingById), new { meetingId = meetingId.Value }, meetingId.Value);
        }

        // Добавить участника (только организатор встречи)
        [HttpPost("{meetingId}/participants"), Authorize]
        public async Task<IActionResult> AddParticipant(Guid meetingId, [FromBody] MeetingAddParticipantDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var result = await meetingService.AddParticipantAsync(meetingId, userId.Value, dto);
            if (!result)
                return Forbid("Недостаточно прав или участник уже добавлен.");

            return Ok();
        }

        // Удалить участника (только организатор встречи)
        [HttpDelete("{meetingId}/participants"), Authorize]
        public async Task<IActionResult> RemoveParticipant(Guid meetingId, [FromBody] MeetingRemoveParticipantDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var result = await meetingService.RemoveParticipantAsync(meetingId, userId.Value, dto);
            if (!result)
                return Forbid("Недостаточно прав или нельзя удалить себя/участник не найден.");

            return Ok();
        }

        // Получить список всех встреч пользователя (по токену)
        [HttpGet("my"), Authorize]
        public async Task<ActionResult<List<MeetingShortDto>>> GetMyMeetings()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var meetings = await meetingService.GetUserMeetingsAsync(userId.Value);
            return Ok(meetings);
        }

        // Получить встречу по ID (доступ только участникам и организатору)
        [HttpGet("{meetingId}"), Authorize]
        public async Task<ActionResult<MeetingDetailDto>> GetMeetingById(Guid meetingId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var meeting = await meetingService.GetMeetingByIdAsync(meetingId, userId.Value);
            if (meeting == null)
                return Forbid("Недостаточно прав или встреча не найдена.");

            return Ok(meeting);
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