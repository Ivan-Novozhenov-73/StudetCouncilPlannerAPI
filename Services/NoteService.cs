using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Interfaces;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Services
{
    public class NoteService(ApplicationDbContext context, IConfiguration configuration) : INoteService
    {
        private readonly string _notesDirectory = configuration.GetValue<string>("NotesDirectory") ?? "App_Data/Notes";
        
        // Проверка, может ли пользователь создавать/редактировать/просматривать заметки (роль 1 или 2 для этого мероприятия)
        public async Task<bool> IsOrganizerOrChiefAsync(Guid eventId, Guid userId)
        {
            var eventUser = await context.EventUsers
                .FirstOrDefaultAsync(eu => eu.EventId == eventId && eu.UserId == userId && (eu.Role == 1 || eu.Role == 2));
            return eventUser != null;
        }

        // Создать заметку
        public async Task<Guid?> CreateNoteAsync(NoteCreateDto dto, Guid authorId)
        {
            CheckRepo();
            // Проверяем, что автор — организатор или главный организатор мероприятия
            if (!await IsOrganizerOrChiefAsync(dto.EventId, authorId))
                return null;

            var noteId = Guid.NewGuid();
            var fileName = $"{noteId}.md";
            var filePath = Path.Combine(_notesDirectory, fileName);

            try
            {
                // Записать файл
                await File.WriteAllTextAsync(filePath, dto.MarkdownContent ?? string.Empty);

                // Сохраняем в БД
                var note = new Note
                {
                    NoteId = noteId,
                    EventId = dto.EventId,
                    UserId = authorId,
                    Title = dto.Title,
                    FilePath = fileName
                };
                context.Notes.Add(note);
                await context.SaveChangesAsync();
                return noteId;
            }
            catch
            {
                return null;
            }
        }

        // Обновить заметку (только организатор/главный организатор)
        public async Task<bool> UpdateNoteAsync(Guid noteId, NoteUpdateDto dto, Guid userId)
        {
            CheckRepo();
            var note = await context.Notes.Include(n => n.Event).FirstOrDefaultAsync(n => n.NoteId == noteId);
            if (note == null) return false;
            // Проверяем право на редактирование
            if (!await IsOrganizerOrChiefAsync(note.EventId, userId))
                return false;

            // Обновить название
            if (!string.IsNullOrWhiteSpace(dto.Title))
                note.Title = dto.Title;

            // Обновить содержимое файла
            var filePath = Path.Combine(_notesDirectory, note.FilePath);
            if (dto.MarkdownContent != null)
            {
                await File.WriteAllTextAsync(filePath, dto.MarkdownContent);
            }

            await context.SaveChangesAsync();
            return true;
        }

        // Получить одну заметку по id (метаданные + содержимое)
        public async Task<NoteDetailDto?> GetNoteByIdAsync(Guid noteId)
        {
            CheckRepo();
            var note = await context.Notes
                .Include(n => n.User)
                .Include(n => n.Event)
                .FirstOrDefaultAsync(n => n.NoteId == noteId);

            if (note == null)
                return null;

            var filePath = Path.Combine(_notesDirectory, note.FilePath);
            string content = "";
            if (File.Exists(filePath))
                content = await File.ReadAllTextAsync(filePath);

            return new NoteDetailDto
            {
                NoteId = note.NoteId,
                EventId = note.EventId,
                EventTitle = note.Event?.Title ?? "",
                AuthorId = note.UserId,
                AuthorFullName = note.User != null ? $"{note.User.Surname} {note.User.Name}" : "",
                Title = note.Title,
                MarkdownContent = content
            };
        }

        // Получить все заметки мероприятия (список)
        public async Task<List<NoteShortDto>> GetNotesByEventAsync(Guid eventId)
        {
            var notes = await context.Notes
                .Where(n => n.EventId == eventId)
                .Include(n => n.User)
                .OrderBy(n => n.Title)
                .ToListAsync();

            return notes.Select(n => new NoteShortDto
            {
                NoteId = n.NoteId,
                Title = n.Title,
                AuthorId = n.UserId,
                AuthorFullName = n.User != null ? $"{n.User.Surname} {n.User.Name}" : ""
            }).ToList();
        }

        private void CheckRepo()
        {
            if (!Directory.Exists(_notesDirectory))
                Directory.CreateDirectory(_notesDirectory);
        }
    }
}