using System;
using System.Collections.Generic;

namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для создания задачи
    public class TaskCreateDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid ExecutorUserId { get; set; }
        public Guid? PartnerId { get; set; }
    }

    // DTO для изменения задачи (только для постановщика)
    public class TaskUpdateDto
    {
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid ExecutorUserId { get; set; }
        public Guid? PartnerId { get; set; }
        public short Status { get; set; }
    }

    // DTO для обновления статуса задачи (для исполнителя)
    public class TaskStatusUpdateDto
    {
        public short Status { get; set; }
    }

    // DTO для просмотра полной информации о задаче
    public class TaskDetailDto
    {
        public Guid TaskId { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public short Status { get; set; }

        // Постановщик
        public Guid CreatorUserId { get; set; }
        public string CreatorUserFullName { get; set; }

        // Исполнитель
        public Guid ExecutorUserId { get; set; }
        public string ExecutorUserFullName { get; set; }

        // Партнёр (если есть)
        public Guid? PartnerId { get; set; }
        public string? PartnerName { get; set; }
    }

    // DTO для краткой информации о задаче (для списков)
    public class TaskShortDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public short Status { get; set; }
        public DateOnly EndDate { get; set; }
    }

    // DTO для фильтрации задач (по пользователю, мероприятию, партнёру)
    public class TaskFilterDto
    {
        public Guid? UserId { get; set; }
        public short? UserRole { get; set; } // 0 - постановщик, 1 - исполнитель (или enum)
        public Guid? EventId { get; set; }
        public Guid? PartnerId { get; set; }
        public List<short>? Statuses { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public bool? SortByEndDateAsc { get; set; }
    }
}