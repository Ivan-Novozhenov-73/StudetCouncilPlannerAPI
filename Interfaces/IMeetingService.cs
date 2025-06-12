using StudetCouncilPlannerAPI.Models.DTOs;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface IMeetingService
{
    Task<Guid?> CreateMeetingAsync(MeetingCreateDto dto, Guid creatorId);
    Task<bool> AddParticipantAsync(Guid meetingId, Guid organizerId, MeetingAddParticipantDto dto);
    Task<bool> RemoveParticipantAsync(Guid meetingId, Guid organizerId, MeetingRemoveParticipantDto dto);
    Task<List<MeetingShortDto>> GetUserMeetingsAsync(Guid userId);
    Task<MeetingDetailDto?> GetMeetingByIdAsync(Guid meetingId, Guid userId);
}