using System.ComponentModel.DataAnnotations;

public class Task
{
    public Guid TaskId { get; set; }

    [Required]
    public Guid EventId { get; set; }

    public Guid? PartnerId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; }

    [Required]
    public short Status { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Навигационные свойства
    public Event Event { get; set; }
    public Partner Partner { get; set; }
    public ICollection<TaskUser> TaskUsers { get; set; }
}