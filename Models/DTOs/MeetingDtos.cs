using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // Для создания встречи
    public class MeetingCreateDto
    {
        [Required, MaxLength(255)]
        public string Title { get; set; }
        [Required, MaxLength(255)]
        public string Location { get; set; }
        [MaxLength(255)]
        public string? Link { get; set; }
        [Required]
        public DateOnly MeetingDate { get; set; }
        [Required]
        public TimeSpan MeetingTime { get; set; }
    }

    // Для добавления участника
    public class MeetingAddParticipantDto
    {
        [Required]
        public Guid UserId { get; set; }
        // Роль не передаём, участник всегда с ролью 0
    }

    // Для удаления участника
    public class MeetingRemoveParticipantDto
    {
        [Required]
        public Guid UserId { get; set; }
    }

    // Краткая информация о встрече (для списка)
    public class MeetingShortDto
    {
        public Guid MeetingId { get; set; }
        public string Title { get; set; }
        public DateOnly MeetingDate { get; set; }
        public TimeSpan MeetingTime { get; set; }
        public string Location { get; set; }
        public string? Link { get; set; }
        public string OrganizerFullName { get; set; }
    }

    // Информация об участнике встречи
    public class MeetingParticipantDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public int Role { get; set; } // 0 — участник, 1 — организатор
    }

    // Детальная информация о встрече
    public class MeetingDetailDto
    {
        public Guid MeetingId { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string? Link { get; set; }
        public DateOnly MeetingDate { get; set; }
        public TimeSpan MeetingTime { get; set; }
        public MeetingParticipantDto Organizer { get; set; }
        public List<MeetingParticipantDto> Participants { get; set; }
    }
}