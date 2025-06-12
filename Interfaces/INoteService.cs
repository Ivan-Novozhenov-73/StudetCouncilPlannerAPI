using StudetCouncilPlannerAPI.Models.DTOs;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface INoteService
{
    Task<bool> IsOrganizerOrChiefAsync(Guid eventId, Guid userId);
    Task<Guid?> CreateNoteAsync(NoteCreateDto dto, Guid authorId);
    Task<bool> UpdateNoteAsync(Guid noteId, NoteUpdateDto dto, Guid userId);
    Task<NoteDetailDto?> GetNoteByIdAsync(Guid noteId);
    Task<List<NoteShortDto>> GetNotesByEventAsync(Guid eventId);
}