using System;
using System.Collections.Generic;

namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для создания заметки
    public class NoteCreateDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
    }

    // DTO для обновления заметки
    public class NoteUpdateDto
    {
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
    }

    // DTO для краткой информации о заметке (например, для списка)
    public class NoteShortDto
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; }
    }

    // DTO для детального просмотра заметки (метаданные + содержимое)
    public class NoteDetailDto
    {
        public Guid NoteId { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public string Title { get; set; }
        public string MarkdownContent { get; set; }
    }

    // DTO для списка заметок мероприятия
    public class NoteListDto
    {
        public Guid EventId { get; set; }
        public List<NoteShortDto> Notes { get; set; }
    }
}