using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Создание задачи
        public async Task<Guid> CreateTaskAsync(TaskCreateDto dto, Guid creatorUserId)
        {
            var task = new Models.Entities.Task
            {
                TaskId = Guid.NewGuid(),
                Title = dto.Title,
                Status = 0,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                EventId = dto.EventId,
                PartnerId = dto.PartnerId,
                TaskUsers = new List<TaskUser>
                {
                    new TaskUser { UserId = creatorUserId, Role = 0 }, // 0 - постановщик
                    new TaskUser { UserId = dto.ExecutorUserId, Role = 1 } // 1 - исполнитель
                }
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task.TaskId;
        }

        // 2. Изменение задачи (для постановщика)
        public async Task<bool> UpdateTaskAsync(Guid taskId, TaskUpdateDto dto, Guid userId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskUsers)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null) return false;

            var isCreator = task.TaskUsers.Any(tu => tu.UserId == userId && tu.Role == 0);
            if (!isCreator) return false;

            task.Title = dto.Title;
            task.StartDate = dto.StartDate;
            task.EndDate = dto.EndDate;
            task.Status = dto.Status;
            task.PartnerId = dto.PartnerId;

            // смена исполнителя
            var executor = task.TaskUsers.FirstOrDefault(tu => tu.Role == 1);
            if (executor != null && executor.UserId != dto.ExecutorUserId)
            {
                task.TaskUsers.Remove(executor);
                task.TaskUsers.Add(new TaskUser { UserId = dto.ExecutorUserId, Role = 1, TaskId = taskId });
            }
            await _context.SaveChangesAsync();
            return true;
        }

        // 3. Изменение статуса задачи (для исполнителя)
        public async Task<bool> UpdateTaskStatusAsync(Guid taskId, TaskStatusUpdateDto dto, Guid userId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskUsers)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null) return false;

            var isExecutor = task.TaskUsers.Any(tu => tu.UserId == userId && tu.Role == 1);
            if (!isExecutor) return false;

            // Исполнитель может только статусы 0, 1, 2
            if (dto.Status < 0 || dto.Status > 2) return false;

            task.Status = dto.Status;
            await _context.SaveChangesAsync();
            return true;
        }

        // 4. Назначить/удалить партнера (только постановщик)
        public async Task<bool> SetPartnerAsync(Guid taskId, Guid? partnerId, Guid userId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskUsers)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null) return false;

            var isCreator = task.TaskUsers.Any(tu => tu.UserId == userId && tu.Role == 0);
            if (!isCreator) return false;

            task.PartnerId = partnerId;
            await _context.SaveChangesAsync();
            return true;
        }

        // 5. Получить задачу по ID (с деталями)
        public async Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskUsers).ThenInclude(tu => tu.User)
                .Include(t => t.Partner)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null) return null;

            var creator = task.TaskUsers.FirstOrDefault(tu => tu.Role == 0)?.User;
            var executor = task.TaskUsers.FirstOrDefault(tu => tu.Role == 1)?.User;

            return new TaskDetailDto
            {
                TaskId = task.TaskId,
                EventId = task.EventId,
                EventTitle = task.Event.Title,
                Title = task.Title,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Status = task.Status,
                CreatorUserId = creator?.UserId ?? Guid.Empty,
                CreatorUserFullName = creator != null ? $"{creator.Surname} {creator.Name}" : "",
                ExecutorUserId = executor?.UserId ?? Guid.Empty,
                ExecutorUserFullName = executor != null ? $"{executor.Surname} {executor.Name}" : "",
                PartnerId = task.PartnerId,
                PartnerName = task.Partner != null ? $"{task.Partner.Surname} {task.Partner.Name}" : null
            };
        }

        // 6. Получить задачи пользователя с фильтрами
        public async Task<List<TaskShortDto>> GetUserTasksAsync(TaskFilterDto filter)
        {
            var query = _context.TaskUsers
                .Where(tu => (!filter.UserId.HasValue || tu.UserId == filter.UserId.Value)
                          && (!filter.UserRole.HasValue || tu.Role == filter.UserRole.Value))
                .Select(tu => tu.Task)
                .Distinct()
                .AsQueryable();

            if (filter.Statuses != null && filter.Statuses.Any())
                query = query.Where(t => filter.Statuses.Contains(t.Status));
            if (filter.EventId.HasValue)
                query = query.Where(t => t.EventId == filter.EventId.Value);
            if (filter.PartnerId.HasValue)
                query = query.Where(t => t.PartnerId == filter.PartnerId.Value);

            query = filter.SortByEndDateAsc == true
                ? query.OrderBy(t => t.EndDate)
                : query.OrderByDescending(t => t.EndDate);

            if (filter.Page.HasValue && filter.PageSize.HasValue)
                query = query.Skip((filter.Page.Value - 1) * filter.PageSize.Value).Take(filter.PageSize.Value);

            return await query.Select(t => new TaskShortDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Status = t.Status,
                EndDate = t.EndDate
            }).ToListAsync();
        }

        // 7. Получить задачи мероприятия
        public async Task<List<TaskShortDto>> GetEventTasksAsync(TaskFilterDto filter)
        {
            var query = _context.Tasks.Where(t => t.EventId == filter.EventId);

            if (filter.Statuses != null && filter.Statuses.Any())
                query = query.Where(t => filter.Statuses.Contains(t.Status));

            query = filter.SortByEndDateAsc == true
                ? query.OrderBy(t => t.EndDate)
                : query.OrderByDescending(t => t.EndDate);

            if (filter.Page.HasValue && filter.PageSize.HasValue)
                query = query.Skip((filter.Page.Value - 1) * filter.PageSize.Value).Take(filter.PageSize.Value);

            return await query.Select(t => new TaskShortDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Status = t.Status,
                EndDate = t.EndDate
            }).ToListAsync();
        }

        // 8. Получить задачи партнера
        public async Task<List<TaskShortDto>> GetPartnerTasksAsync(TaskFilterDto filter)
        {
            var query = _context.Tasks.Where(t => t.PartnerId == filter.PartnerId);

            if (filter.Statuses != null && filter.Statuses.Any())
                query = query.Where(t => filter.Statuses.Contains(t.Status));

            query = filter.SortByEndDateAsc == true
                ? query.OrderBy(t => t.EndDate)
                : query.OrderByDescending(t => t.EndDate);

            if (filter.Page.HasValue && filter.PageSize.HasValue)
                query = query.Skip((filter.Page.Value - 1) * filter.PageSize.Value).Take(filter.PageSize.Value);

            return await query.Select(t => new TaskShortDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Status = t.Status,
                EndDate = t.EndDate
            }).ToListAsync();
        }
    }
}