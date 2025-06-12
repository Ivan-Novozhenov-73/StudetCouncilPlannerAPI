using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;
using StudetCouncilPlannerAPI.Models.Dtos;

namespace StudetCouncilPlannerAPI.Services
{
    public class PartnerService(ApplicationDbContext context)
    {
        // Создать партнера
        public async Task<Guid> CreatePartnerAsync(PartnerCreateDto dto)
        {
            var partner = new Partner
            {
                PartnerId = Guid.NewGuid(),
                Surname = dto.Surname,
                Name = dto.Name,
                Patronymic = dto.Patronymic,
                Description = dto.Description,
                Phone = dto.Phone,
                Contacts = dto.Contacts,
                Archive = false
            };
            context.Partners.Add(partner);
            await context.SaveChangesAsync();
            return partner.PartnerId;
        }

        // Обновить партнера
        public async Task<bool> UpdatePartnerAsync(Guid partnerId, PartnerUpdateDto dto)
        {
            var partner = await context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
            if (partner == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Surname)) partner.Surname = dto.Surname;
            if (!string.IsNullOrWhiteSpace(dto.Name)) partner.Name = dto.Name;
            if (dto.Patronymic != null) partner.Patronymic = dto.Patronymic;
            if (!string.IsNullOrWhiteSpace(dto.Description)) partner.Description = dto.Description;
            if (dto.Phone.HasValue) partner.Phone = dto.Phone.Value;
            if (!string.IsNullOrWhiteSpace(dto.Contacts)) partner.Contacts = dto.Contacts;

            await context.SaveChangesAsync();
            return true;
        }

        // Архивировать партнера
        public async Task<bool> ArchivePartnerAsync(Guid partnerId)
        {
            var partner = await context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
            if (partner == null || partner.Archive) return false;

            partner.Archive = true;
            await context.SaveChangesAsync();
            return true;
        }

        // Восстановить из архива
        public async Task<bool> RestorePartnerAsync(Guid partnerId)
        {
            var partner = await context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
            if (partner == null || !partner.Archive) return false;

            partner.Archive = false;
            await context.SaveChangesAsync();
            return true;
        }

        // Поиск и фильтрация
        public async Task<List<PartnerShortDto>> SearchPartnersAsync(PartnerFilterDto filter)
        {
            var query = context.Partners.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FioSearch))
            {
                var fio = filter.FioSearch.ToLower();
                query = query.Where(p =>
                    (p.Surname + " " + p.Name + (p.Patronymic != null ? " " + p.Patronymic : "")).ToLower().Contains(fio));
            }

            if (filter.Archive.HasValue)
                query = query.Where(p => p.Archive == filter.Archive.Value);

            if (filter.EventId.HasValue)
                query = query.Where(p => p.EventPartners.Any(ep => ep.EventId == filter.EventId.Value));

            if (filter.Page.HasValue && filter.PageSize.HasValue)
                query = query.Skip((filter.Page.Value - 1) * filter.PageSize.Value).Take(filter.PageSize.Value);

            return await query
                .Select(p => new PartnerShortDto
                {
                    PartnerId = p.PartnerId,
                    Fio = p.Surname + " " + p.Name + (p.Patronymic != null ? " " + p.Patronymic : ""),
                    Description = p.Description
                })
                .ToListAsync();
        }

        // Получить партнера по id (детально)
        public async Task<PartnerDetailDto?> GetPartnerByIdAsync(Guid partnerId)
        {
            var partner = await context.Partners
                .Include(p => p.EventPartners).ThenInclude(ep => ep.Event)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.PartnerId == partnerId);

            if (partner == null) return null;

            return new PartnerDetailDto
            {
                PartnerId = partner.PartnerId,
                Surname = partner.Surname,
                Name = partner.Name,
                Patronymic = partner.Patronymic,
                Description = partner.Description,
                Phone = partner.Phone,
                Contacts = partner.Contacts,
                Archive = partner.Archive,
                Events = partner.EventPartners?
                    .Select(ep => new EventShortDto
                    {
                        EventId = ep.EventId,
                        Title = ep.Event.Title,
                        StartDate = ep.Event.StartDate,
                        EndDate = ep.Event.EndDate
                    }).ToList(),
                Tasks = partner.Tasks?
                    .Select(t => new TaskShortDto
                    {
                        TaskId = t.TaskId,
                        Title = t.Title,
                        Status = t.Status,
                        EndDate = t.EndDate
                    }).ToList()
            };
        }

        // Получить все задачи партнера
        public async Task<List<TaskShortDto>> GetTasksByPartnerAsync(Guid partnerId)
        {
            return await context.Tasks
                .Where(t => t.PartnerId == partnerId)
                .Select(t => new TaskShortDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Status = t.Status,
                    EndDate = t.EndDate
                })
                .ToListAsync();
        }

        // Получить все мероприятия партнера
        public async Task<List<EventShortDto>> GetEventsByPartnerAsync(Guid partnerId)
        {
            return await context.EventPartners
                .Where(ep => ep.PartnerId == partnerId)
                .Select(ep => new EventShortDto
                {
                    EventId = ep.EventId,
                    Title = ep.Event.Title,
                    StartDate = ep.Event.StartDate,
                    EndDate = ep.Event.EndDate
                })
                .ToListAsync();
        }
    }
}