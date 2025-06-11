using System;
using System.Collections.Generic;

namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для строки плана мероприятий на месяц
    public class EventPlanReportRowDto
    {
        public string Title { get; set; }
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public string ShortDescription { get; set; }
        public string ResponsibleFullName { get; set; }
        public string ResponsiblePhone { get; set; }
    }

    // DTO для строки отчета о проведенных мероприятиях
    public class EventCompletedReportRowDto
    {
        public string Title { get; set; }
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
        public int OrganizersCount { get; set; }
        public int ActiveParticipantsCount { get; set; }
        public short? NumberOfSpectators { get; set; }
        public string ResponsibleFullName { get; set; }
        public string ResponsiblePhone { get; set; }
        public List<string> OrganizerTeamFullNames { get; set; }
    }

    // DTO для строки отчета о мероприятиях пользователя
    public class UserEventsReportRowDto
    {
        public string Title { get; set; }
        public short UserRole { get; set; } // 0 — участник, 1 — организатор, 2 — главный организатор
        public DateOnly? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string? Location { get; set; }
    }

    // DTO для хедера PDF отчета пользователя
    public class UserInfoForReportDto
    {
        public string Group { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
    }
}