using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        [Required, MaxLength(255)] public string Login { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string Surname { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
        [MaxLength(255)] public string? Patronymic { get; set; }
        [Required] public short Role { get; set; }
        [Required, MaxLength(12)] public string Group { get; set; } = string.Empty;
        [Required] public long Phone { get; set; }
        [Required, MaxLength(255)] public string Contacts { get; set; } = string.Empty;
        [Required] public bool Archive { get; set; }
        // Навигационные свойства
        public ICollection<EventUser> EventUsers { get; set; } = [];
        public ICollection<TaskUser> TaskUsers { get; set; } = [];
        public ICollection<MeetingUser> MeetingUsers { get; set; } = [];
        public ICollection<Note> Notes { get; set; } = [];
    }
}