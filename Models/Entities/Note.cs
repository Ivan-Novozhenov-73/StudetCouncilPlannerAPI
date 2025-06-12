using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class Note
    {
        public Guid NoteId { get; set; }
        [Required] public Guid UserId { get; set; }
        [Required] public Guid EventId { get; set; }
        [Required, MaxLength(255)] public string Title { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string FilePath { get; set; } = string.Empty;
        // Навигационные свойства
        public User User { get; set; } = new();
        public Event Event { get; set; } = new();
    }
}