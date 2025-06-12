namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для строки плана мероприятий на месяц
    public class EventPlanReportRowDto
    {
        public string Title { get; set; } = string.Empty;
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public string ResponsibleFullName { get; set; } = string.Empty;
        public string ResponsiblePhone { get; set; } = string.Empty;
    }

    // DTO для строки отчета о проведенных мероприятиях
    public class EventCompletedReportRowDto
    {
        public string Title { get; set; } = string.Empty;
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public int OrganizersCount { get; set; }
        public int ActiveParticipantsCount { get; set; }
        public short? NumberOfSpectators { get; set; }
        public string ResponsibleFullName { get; set; } = string.Empty;
        public string ResponsiblePhone { get; set; } = string.Empty;
        public List<string> OrganizerTeamFullNames { get; set; } = [];
    }

    // DTO для строки отчета о мероприятиях пользователя
    public class UserEventsReportRowDto
    {
        public string Title { get; set; } = string.Empty;
        public short UserRole { get; set; } // 0 — участник, 1 — организатор, 2 — главный организатор
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
    }

    // DTO для хедера PDF отчета пользователя
    public class UserInfoForReportDto
    {
        public string Group { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;
    }
}