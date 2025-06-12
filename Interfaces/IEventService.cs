using StudetCouncilPlannerAPI.Models.Dtos;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface IEventService
{
    Task<List<EventShortDto>> GetEventsAsync(EventListQueryDto query);
    Task<EventDetailDto?> GetEventByIdAsync(Guid eventId);
    Task<Guid> CreateEventAsync(EventCreateDto dto, Guid creatorUserId);
    Task<bool> UpdateEventAsync(Guid eventId, EventUpdateDto dto);
    Task<bool> AddParticipantAsync(Guid eventId, Guid userId);
    Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId);
    Task<bool> AddOrganizerAsync(Guid eventId, Guid userId);
    Task<bool> RemoveOrganizerAsync(Guid eventId, Guid userId);
    Task<bool> AddPartnerAsync(Guid eventId, Guid partnerId);
    Task<bool> RemovePartnerAsync(Guid eventId, Guid partnerId);
    Task<List<EventShortDto>> GetUserEventsAsync(Guid userId);
    Task<List<UserShortDto>> GetOrganizersAsync(Guid eventId);
    Task<List<UserShortDto>> GetParticipantsAsync(Guid eventId);
    Task<List<EventPartnerShortDto>> GetPartnersAsync(Guid eventId);
}