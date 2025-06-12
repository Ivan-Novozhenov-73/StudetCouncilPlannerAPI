using StudetCouncilPlannerAPI.Models.DTOs;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface ITaskService
{
    Task<Guid> CreateTaskAsync(TaskCreateDto dto, Guid creatorUserId);
    Task<bool> UpdateTaskAsync(Guid taskId, TaskUpdateDto dto, Guid userId);
    Task<bool> UpdateTaskStatusAsync(Guid taskId, TaskStatusUpdateDto dto, Guid userId);
    Task<bool> SetPartnerAsync(Guid taskId, Guid? partnerId, Guid userId);
    Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId);
    Task<List<TaskShortDto>> GetUserTasksAsync(TaskFilterDto filter);
    Task<List<TaskShortDto>> GetEventTasksAsync(TaskFilterDto filter);
    Task<List<TaskShortDto>> GetPartnerTasksAsync(TaskFilterDto filter);
}