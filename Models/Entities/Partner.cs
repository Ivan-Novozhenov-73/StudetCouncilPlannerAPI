using System.ComponentModel.DataAnnotations;

namespace StudetCouncilPlannerAPI.Models.Entities
{
    public class Partner
    {
        public Guid PartnerId { get; set; }
        [Required, MaxLength(255)] public string Surname { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
        [MaxLength(255)] public string? Patronymic { get; set; }
        [Required, MaxLength(255)] public string Description { get; set; } = string.Empty;
        [Required] public long Phone { get; set; }
        [Required, MaxLength(255)] public string Contacts { get; set; } = string.Empty;
        [Required] public bool Archive { get; set; }
        // Навигационные свойства
        public ICollection<EventPartner> EventPartners { get; set; } = [];
        public ICollection<Task> Tasks { get; set; } = [];
    }
}