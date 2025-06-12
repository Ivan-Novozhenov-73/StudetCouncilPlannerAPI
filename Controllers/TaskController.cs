using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;
using System.Security.Claims;
using StudetCouncilPlannerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]"), Authorize]
    public class TaskController(TaskService taskService, ApplicationDbContext context) : ControllerBase
    {
        // Получить задачу по ID
        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskDetailDto>> GetById(Guid taskId)
        {
            var result = await taskService.GetTaskByIdAsync(taskId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Создать задачу
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TaskCreateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var user = await context.Users.FindAsync(currentUserId);
            Console.WriteLine($"[TaskController.Create] user: {user}, user.Role: {user?.Role}");
            if (user == null || (user.Role != 1 && user.Role != 2))
                return Forbid();

            bool isMainOrganizer = await context.EventUsers
                .AnyAsync(eu => eu.EventId == dto.EventId && eu.UserId == currentUserId && eu.Role == 2);
            if (!isMainOrganizer)
                return Forbid();

            bool executorIsAllowed = await context.EventUsers
                .AnyAsync(eu => eu.EventId == dto.EventId && eu.UserId == dto.ExecutorUserId && (eu.Role == 1 || eu.Role == 2));
            if (!executorIsAllowed)
                return BadRequest("Executor must be organizer or main organizer of event.");

            var taskId = await taskService.CreateTaskAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { taskId }, taskId);
        }

        // Изменить задачу (для постановщика)
        [HttpPut("{taskId}")]
        public async Task<ActionResult> Update(Guid taskId, [FromBody] TaskUpdateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await taskService.UpdateTaskAsync(taskId, dto, currentUserId);
            if (!result) return Forbid();
            return NoContent();
        }

        // Изменить статус задачи (для исполнителя)
        [HttpPatch("{taskId}/status")]
        public async Task<ActionResult> UpdateStatus(Guid taskId, [FromBody] TaskStatusUpdateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await taskService.UpdateTaskStatusAsync(taskId, dto, currentUserId);
            if (!result) return Forbid();
            return NoContent();
        }

        // Назначить/удалить партнёра
        [HttpPatch("{taskId}/partner")]
        public async Task<ActionResult> SetPartner(Guid taskId, [FromQuery] Guid? partnerId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await taskService.SetPartnerAsync(taskId, partnerId, currentUserId);
            if (!result) return Forbid();
            return NoContent();
        }

        // Получить задачи пользователя с фильтрами
        [HttpGet("user")]
        public async Task<ActionResult<List<TaskShortDto>>> GetUserTasks([FromQuery] Guid userId, [FromQuery] short? userRole, [FromQuery] List<short>? statuses, [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] bool? sortByEndDateAsc)
        {
            var filter = new TaskFilterDto
            {
                UserId = userId,
                UserRole = userRole,
                Statuses = statuses,
                Page = page,
                PageSize = pageSize,
                SortByEndDateAsc = sortByEndDateAsc
            };
            var result = await taskService.GetUserTasksAsync(filter);
            return Ok(result);
        }

        // Получить задачи мероприятия
        [HttpGet("event")]
        public async Task<ActionResult<List<TaskShortDto>>> GetEventTasks([FromQuery] Guid eventId, [FromQuery] List<short>? statuses, [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] bool? sortByEndDateAsc)
        {
            var filter = new TaskFilterDto
            {
                EventId = eventId,
                Statuses = statuses,
                Page = page,
                PageSize = pageSize,
                SortByEndDateAsc = sortByEndDateAsc
            };
            var result = await taskService.GetEventTasksAsync(filter);
            return Ok(result);
        }

        // Получить задачи партнёра
        [HttpGet("partner")]
        public async Task<ActionResult<List<TaskShortDto>>> GetPartnerTasks([FromQuery] Guid partnerId, [FromQuery] List<short>? statuses, [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] bool? sortByEndDateAsc)
        {
            var filter = new TaskFilterDto
            {
                PartnerId = partnerId,
                Statuses = statuses,
                Page = page,
                PageSize = pageSize,
                SortByEndDateAsc = sortByEndDateAsc
            };
            var result = await taskService.GetPartnerTasksAsync(filter);
            return Ok(result);
        }

        // Вспомогательный метод для получения текущего пользователя
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
    }
}