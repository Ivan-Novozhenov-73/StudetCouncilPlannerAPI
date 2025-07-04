using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class Event
    {
        public Guid EventId { get; set; }
        [Required, MaxLength(255)] public string Title { get; set; } = string.Empty;
        [Required] public short Status { get; set; }
        [Required, MaxLength(255)] public string Description { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        [MaxLength(255)] public string? Location { get; set; }
        public short? NumberOfParticipants { get; set; }
        // Навигационные свойства
        public ICollection<Task> Tasks { get; set; } = [];
        public ICollection<Note> Notes { get; set; } = [];
        public ICollection<EventUser> EventUsers { get; set; } = [];
        public ICollection<EventPartner> EventPartners { get; set; } = [];
    }
}