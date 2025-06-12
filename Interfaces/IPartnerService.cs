using StudetCouncilPlannerAPI.Models.Dtos;
using StudetCouncilPlannerAPI.Models.DTOs;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface IPartnerService
{
    Task<Guid> CreatePartnerAsync(PartnerCreateDto dto);
    Task<bool> UpdatePartnerAsync(Guid partnerId, PartnerUpdateDto dto);
    Task<bool> ArchivePartnerAsync(Guid partnerId);
    Task<bool> RestorePartnerAsync(Guid partnerId);
    Task<List<PartnerShortDto>> SearchPartnersAsync(PartnerFilterDto filter);
    Task<PartnerDetailDto?> GetPartnerByIdAsync(Guid partnerId);
    Task<List<TaskShortDto>> GetTasksByPartnerAsync(Guid partnerId);
    Task<List<EventShortDto>> GetEventsByPartnerAsync(Guid partnerId);
}