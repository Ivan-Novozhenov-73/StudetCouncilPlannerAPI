using System.ComponentModel.DataAnnotations;

public class Note
{
    public Guid NoteId { get; set; }

    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid EventId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; }

    [Required, MaxLength(255)]
    public string FilePath { get; set; }

    // Навигационные свойства
    public User User { get; set; }
    public Event Event { get; set; }
}