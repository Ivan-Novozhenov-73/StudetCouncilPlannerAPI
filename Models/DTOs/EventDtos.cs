namespace StudetCouncilPlannerAPI.Models.Dtos
{
    // Краткая информация о мероприятии
    public class EventShortDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public short Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Location { get; set; }
        public TimeSpan? EventTime { get; set; }
        public short? NumberOfParticipants { get; set; }
    }

    // Детальная информация о мероприятии
    public class EventDetailDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public short Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public short? NumberOfParticipants { get; set; }

        public List<EventTaskShortDto> Tasks { get; set; }
        public List<EventNoteShortDto> Notes { get; set; }
        public List<UserShortDto> Organizers { get; set; }
        public List<UserShortDto> Participants { get; set; }
        public List<EventPartnerShortDto> Partners { get; set; }
    }

    // Для создания мероприятия
    public class EventCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public short? NumberOfParticipants { get; set; }
        // Можно добавить списки Id для участников/организаторов/партнеров, если нужно
    }

    // Для обновления мероприятия
    public class EventUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public short Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public short? NumberOfParticipants { get; set; }
    }

    // DTO для фильтрации/пагинации
    public class EventListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public short? Status { get; set; }
        public string? Search { get; set; }
        public bool? IsActual { get; set; }
    }

    // Краткая информация для связанных сущностей
    public class EventTaskShortDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
    public class EventNoteShortDto
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
    public class UserShortDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public short Role { get; set; }
    }
    public class EventPartnerShortDto
    {
        public Guid PartnerId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}