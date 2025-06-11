using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Services;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventReportController : ControllerBase
    {
        private readonly EventReportService _eventReportService;

        public EventReportController(EventReportService eventReportService)
        {
            _eventReportService = eventReportService;
        }

        /// <summary>
        /// План мероприятий на следующий месяц (PDF) — только роль 2
        /// </summary>
        [HttpGet("plan-next-month")]
        [Authorize]
        public async Task<IActionResult> GetPlanNextMonthPdf()
        {
            if (!UserHasRole(2))
                return Forbid("Доступ разрешен только пользователям с ролью 2.");

            var pdfBytes = await _eventReportService.GeneratePlanNextMonthAsync();
            return File(pdfBytes, "application/pdf", $"plan_{DateTime.Today.AddMonths(1):yyyy_MM}.pdf");
        }

        /// <summary>
        /// Отчет о завершенных мероприятиях за месяц (PDF) — только роль 2
        /// </summary>
        [HttpGet("completed")]
        [Authorize]
        public async Task<IActionResult> GetCompletedEventsReport([FromQuery] int year, [FromQuery] int month)
        {
            if (!UserHasRole(2))
                return Forbid("Доступ разрешен только пользователям с ролью 2.");

            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest("Некорректная дата.");

            var pdfBytes = await _eventReportService.GenerateReportForMonthAsync(year, month);
            return File(pdfBytes, "application/pdf", $"completed_{year}_{month:00}.pdf");
        }

        /// <summary>
        /// Отчет о завершенных мероприятиях пользователя (PDF)
        /// </summary>
        [HttpGet("my-events")]
        [Authorize]
        public async Task<IActionResult> GetUserEventsReport()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var pdfBytes = await _eventReportService.GenerateUserEventsReportAsync(userId.Value);
            return File(pdfBytes, "application/pdf", $"my_events_{DateTime.Today:yyyyMMdd}.pdf");
        }

        // Получить идентификатор пользователя из токена
        private Guid? GetUserIdFromToken()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }

        // Проверить наличие нужной роли (roleClaimValue == 2)
        private bool UserHasRole(short requiredRole)
        {
            // Предполагается, что роль хранится в клейме "role" или "Role"
            var roleClaim = User.FindFirst("role") ?? User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
                return false;
            if (short.TryParse(roleClaim.Value, out short userRole))
                return userRole == requiredRole;
            return false;
        }
    }
}