using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        // Получить задачу по ID
        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskDetailDto>> GetById(Guid taskId)
        {
            var result = await _taskService.GetTaskByIdAsync(taskId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Создать задачу
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TaskCreateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null) return Forbid();

            if (user.Role != 1 && user.Role != 2)
                return Forbid();

            var taskId = await _taskService.CreateTaskAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { taskId }, taskId);
        }

        // Изменить задачу (для постановщика)
        [HttpPut("{taskId}")]
        public async Task<ActionResult> Update(Guid taskId, [FromBody] TaskUpdateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _taskService.UpdateTaskAsync(taskId, dto, currentUserId);
            if (!result) return Forbid();
            return NoContent();
        }

        // Изменить статус задачи (для исполнителя)
        [HttpPatch("{taskId}/status")]
        public async Task<ActionResult> UpdateStatus(Guid taskId, [FromBody] TaskStatusUpdateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _taskService.UpdateTaskStatusAsync(taskId, dto, currentUserId);
            if (!result) return Forbid();
            return NoContent();
        }

        // Назначить/удалить партнёра
        [HttpPatch("{taskId}/partner")]
        public async Task<ActionResult> SetPartner(Guid taskId, [FromQuery] Guid? partnerId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _taskService.SetPartnerAsync(taskId, partnerId, currentUserId);
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
            var result = await _taskService.GetUserTasksAsync(filter);
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
            var result = await _taskService.GetEventTasksAsync(filter);
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
            var result = await _taskService.GetPartnerTasksAsync(filter);
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