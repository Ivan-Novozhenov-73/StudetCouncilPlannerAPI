namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для создания заметки
    public class NoteCreateDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MarkdownContent { get; set; } = string.Empty;
    }

    // DTO для обновления заметки
    public class NoteUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string MarkdownContent { get; set; } = string.Empty;
    }

    // DTO для краткой информации о заметке (например, для списка)
    public class NoteShortDto
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; } = string.Empty;
    }

    // DTO для детального просмотра заметки (метаданные + содержимое)
    public class NoteDetailDto
    {
        public Guid NoteId { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MarkdownContent { get; set; } = string.Empty;
    }

    // DTO для списка заметок мероприятия
    public class NoteListDto
    {
        public Guid EventId { get; set; }
        public List<NoteShortDto> Notes { get; set; } = [];
    }
}