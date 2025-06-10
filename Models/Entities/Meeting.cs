using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class Meeting
    {
        public Guid MeetingId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public DateOnly MeetingDate { get; set; }

        [Required]
        public TimeSpan MeetingTime { get; set; }

        [Required, MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string? Link { get; set; }

        // Навигационные свойства
        public ICollection<MeetingUser> MeetingUsers { get; set; }
    }
}