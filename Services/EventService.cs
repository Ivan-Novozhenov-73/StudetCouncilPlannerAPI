using StudetCouncilPlannerAPI.Models.Entities;
using StudetCouncilPlannerAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;

namespace StudetCouncilPlannerAPI.Services
{
    public class EventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получить список мероприятий (с фильтрацией, пагинацией, сортировкой)
        public async Task<List<EventShortDto>> GetEventsAsync(EventListQueryDto query)
        {
            var eventsQuery = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(query.Search))
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(query.Search) || e.Description.Contains(query.Search));

            if (query.Status.HasValue)
                eventsQuery = eventsQuery.Where(e => e.Status == query.Status.Value);

            if (query.IsActual.HasValue)
            {
                if (query.IsActual.Value)
                    eventsQuery = eventsQuery.Where(e => e.Status >= 0 && e.Status <= 3);
                else
                    eventsQuery = eventsQuery.Where(e => e.Status == 4 || e.Status == 5);
            }

            eventsQuery = eventsQuery.OrderBy(e => e.EndDate);

            var events = await eventsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            // Преобразование в DTO
            return events.Select(e => new EventShortDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Status = e.Status,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                EventTime = e.EventTime,
                NumberOfParticipants = e.NumberOfParticipants
            }).ToList();
        }

        // Получить мероприятие по id
        public async Task<EventDetailDto?> GetEventByIdAsync(Guid eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Tasks)
                .Include(e => e.Notes)
                .Include(e => e.EventUsers).ThenInclude(eu => eu.User)
                .Include(e => e.EventPartners).ThenInclude(ep => ep.Partner)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null) return null;

            // Разделение участников и организаторов по роли
            var organizers = ev.EventUsers
                .Where(eu => eu.Role == 1) // 1 — организатор
                .Select(eu => new UserShortDto
                {
                    UserId = eu.UserId,
                    FullName = eu.User.Surname + " " + eu.User.Name,
                    Role = eu.Role
                }).ToList();

            var participants = ev.EventUsers
                .Where(eu => eu.Role == 0) // 0 — участник
                .Select(eu => new UserShortDto
                {
                    UserId = eu.UserId,
                    FullName = eu.User.Surname + " " + eu.User.Name,
                    Role = eu.Role
                }).ToList();

            return new EventDetailDto
            {
                EventId = ev.EventId,
                Title = ev.Title,
                Status = ev.Status,
                Description = ev.Description,
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                EventTime = ev.EventTime,
                Location = ev.Location,
                NumberOfParticipants = ev.NumberOfParticipants,
                Tasks = ev.Tasks.Select(t => new EventTaskShortDto { TaskId = t.TaskId, Title = t.Title }).ToList(),
                Notes = ev.Notes.Select(n => new NoteShortDto { NoteId = n.NoteId, Title = n.Title }).ToList(),
                Organizers = organizers,
                Participants = participants,
                Partners = ev.EventPartners.Select(ep => new EventPartnerShortDto
                {
                    PartnerId = ep.PartnerId,
                    Name = ep.Partner.Name + " " + ep.Partner.Surname
                }).ToList()
            };
        }

        // Создать мероприятие
        public async Task<Guid> CreateEventAsync(EventCreateDto dto, Guid creatorUserId)
        {
            var ev = new Event
            {
                EventId = Guid.NewGuid(),
                Title = dto.Title,
                Status = 0, // Входящее по умолчанию
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                EventTime = dto.EventTime,
                Location = dto.Location,
                NumberOfParticipants = dto.NumberOfParticipants,
                EventUsers = new List<EventUser>
                {
                    new EventUser
                    {
                        UserId = creatorUserId,
                        Role = 2 // 2 — главный организатор
                    }
                }
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return ev.EventId;
        }

        // Изменить мероприятие
        public async Task<bool> UpdateEventAsync(Guid eventId, EventUpdateDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null) return false;

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.Status = dto.Status;
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.EventTime = dto.EventTime;
            ev.Location = dto.Location;
            ev.NumberOfParticipants = dto.NumberOfParticipants;

            await _context.SaveChangesAsync();
            return true;
        }

        // Добавить участника
        public async Task<bool> AddParticipantAsync(Guid eventId, Guid userId)
        {
            var exists = await _context.EventUsers.AnyAsync(eu => eu.EventId == eventId && eu.UserId == userId);
            if (exists) return false;

            _context.EventUsers.Add(new EventUser
            {
                EventId = eventId,
                UserId = userId,
                Role = 0 // участник
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // Удалить участника
        public async Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId)
        {
            var eu = await _context.EventUsers.FirstOrDefaultAsync(eu => eu.EventId == eventId && eu.UserId == userId && eu.Role == 0);
            if (eu == null) return false;

            _context.EventUsers.Remove(eu);
            await _context.SaveChangesAsync();
            return true;
        }

        // Добавить организатора
        public async Task<bool> AddOrganizerAsync(Guid eventId, Guid userId)
        {
            var exists = await _context.EventUsers.AnyAsync(eu => eu.EventId == eventId && eu.UserId == userId);
            if (exists) return false;

            _context.EventUsers.Add(new EventUser
            {
                EventId = eventId,
                UserId = userId,
                Role = 1 // организатор
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // Удалить организатора
        public async Task<bool> RemoveOrganizerAsync(Guid eventId, Guid userId)
        {
            var eu = await _context.EventUsers.FirstOrDefaultAsync(eu => eu.EventId == eventId && eu.UserId == userId && eu.Role == 1);
            if (eu == null) return false;

            _context.EventUsers.Remove(eu);
            await _context.SaveChangesAsync();
            return true;
        }

        // Добавить партнера
        public async Task<bool> AddPartnerAsync(Guid eventId, Guid partnerId)
        {
            var exists = await _context.EventPartners.AnyAsync(ep => ep.EventId == eventId && ep.PartnerId == partnerId);
            if (exists) return false;

            _context.EventPartners.Add(new EventPartner
            {
                EventId = eventId,
                PartnerId = partnerId
            });
            await _context.SaveChangesAsync();
            return true;
        }

        // Удалить партнера
        public async Task<bool> RemovePartnerAsync(Guid eventId, Guid partnerId)
        {
            var ep = await _context.EventPartners.FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.PartnerId == partnerId);
            if (ep == null) return false;

            _context.EventPartners.Remove(ep);
            await _context.SaveChangesAsync();
            return true;
        }

        // Получить мероприятия пользователя
        public async Task<List<EventShortDto>> GetUserEventsAsync(Guid userId)
        {
            var events = await _context.EventUsers
                .Where(eu => eu.UserId == userId)
                .Select(eu => eu.Event)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            return events.Select(e => new EventShortDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Status = e.Status,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                EventTime = e.EventTime,
                NumberOfParticipants = e.NumberOfParticipants
            }).ToList();
        }

        // Получить список организаторов мероприятия
        public async Task<List<UserShortDto>> GetOrganizersAsync(Guid eventId)
        {
            var organizers = await _context.EventUsers
                .Where(eu => eu.EventId == eventId && eu.Role == 1)
                .Include(eu => eu.User)
                .Select(eu => new UserShortDto
                {
                    UserId = eu.UserId,
                    FullName = eu.User.Surname + " " + eu.User.Name,
                    Role = eu.Role
                }).ToListAsync();

            return organizers;
        }

        // Получить список участников мероприятия
        public async Task<List<UserShortDto>> GetParticipantsAsync(Guid eventId)
        {
            var participants = await _context.EventUsers
                .Where(eu => eu.EventId == eventId && eu.Role == 0)
                .Include(eu => eu.User)
                .Select(eu => new UserShortDto
                {
                    UserId = eu.UserId,
                    FullName = eu.User.Surname + " " + eu.User.Name,
                    Role = eu.Role
                }).ToListAsync();

            return participants;
        }

        // Получить список партнеров мероприятия
        public async Task<List<EventPartnerShortDto>> GetPartnersAsync(Guid eventId)
        {
            var partners = await _context.EventPartners
                .Where(ep => ep.EventId == eventId)
                .Include(ep => ep.Partner)
                .Select(ep => new EventPartnerShortDto
                {
                    PartnerId = ep.PartnerId,
                    Name = ep.Partner.Name + " " + ep.Partner.Surname
                }).ToListAsync();

            return partners;
        }
    }
}