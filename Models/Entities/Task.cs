using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class Task
    {
        public Guid TaskId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        public Guid? PartnerId { get; set; }

        [Required, MaxLength(255)]
        public required string Title { get; set; }

        [Required]
        public short Status { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        // Навигационные свойства
        public required Event Event { get; set; }
        public required Partner Partner { get; set; }
        public required ICollection<TaskUser> TaskUsers { get; set; }
    }
}