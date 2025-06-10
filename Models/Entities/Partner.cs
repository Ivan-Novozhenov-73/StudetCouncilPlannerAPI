using System.ComponentModel.DataAnnotations;

public class Partner
{
    public Guid PartnerId { get; set; }

    [Required, MaxLength(255)]
    public string Surname { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Patronymic { get; set; }

    [Required, MaxLength(255)]
    public string Description { get; set; }

    [Required]
    public long Phone { get; set; }

    [Required, MaxLength(255)]
    public string Contacts { get; set; }

    [Required]
    public bool Archive { get; set; } = false;

    // Навигационные свойства
    public ICollection<EventPartner> EventPartners { get; set; }
    public ICollection<Task> Tasks { get; set; }
}