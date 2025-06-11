using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Services
{
    public class MeetingService
    {
        private readonly ApplicationDbContext _context;

        public MeetingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Проверка глобальной роли пользователя (1 — глава, 2 — председатель)
        private async Task<bool> IsUserCouncilHeadOrChairAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && (user.Role == 1 || user.Role == 2);
        }

        // Получить организатора встречи (MeetingUser с Role == 1)
        private async Task<MeetingUser?> GetMeetingOrganizerAsync(Guid meetingId)
        {
            return await _context.MeetingUsers
                .Include(mu => mu.User)
                .FirstOrDefaultAsync(mu => mu.MeetingId == meetingId && mu.Role == 1);
        }

        // Проверка, является ли пользователь организатором данной встречи
        public async Task<bool> IsOrganizerAsync(Guid meetingId, Guid userId)
        {
            return await _context.MeetingUsers.AnyAsync(mu =>
                mu.MeetingId == meetingId && mu.UserId == userId && mu.Role == 1);
        }

        // Проверка, является ли пользователь участником или организатором встречи
        public async Task<bool> IsParticipantOrOrganizerAsync(Guid meetingId, Guid userId)
        {
            return await _context.MeetingUsers.AnyAsync(mu =>
                mu.MeetingId == meetingId && mu.UserId == userId);
        }

        // Создание встречи (только глава или председатель студсовета)
        public async Task<Guid?> CreateMeetingAsync(MeetingCreateDto dto, Guid creatorId)
        {
            if (!await IsUserCouncilHeadOrChairAsync(creatorId))
                return null;

            var meeting = new Meeting
            {
                MeetingId = Guid.NewGuid(),
                Title = dto.Title,
                Location = dto.Location,
                Link = dto.Link,
                MeetingDate = dto.MeetingDate,
                MeetingTime = dto.MeetingTime,
                MeetingUsers = new List<MeetingUser>()
            };

            // Добавляем организатора (создателя)
            meeting.MeetingUsers.Add(new MeetingUser
            {
                MeetingId = meeting.MeetingId,
                UserId = creatorId,
                Role = 1 // организатор
            });

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();
            return meeting.MeetingId;
        }

        // Добавление участника (только организатор, нельзя добавить второго организатора)
        public async Task<bool> AddParticipantAsync(Guid meetingId, Guid organizerId, MeetingAddParticipantDto dto)
        {
            // Проверяем, что organizerId действительно организатор этой встречи
            if (!await IsOrganizerAsync(meetingId, organizerId))
                return false;

            // Нельзя добавить организатора или дублировать участника
            if (await _context.MeetingUsers.AnyAsync(mu => mu.MeetingId == meetingId && mu.UserId == dto.UserId))
                return false;

            // Добавляем участника (роль 0)
            var meetingUser = new MeetingUser
            {
                MeetingId = meetingId,
                UserId = dto.UserId,
                Role = 0
            };
            _context.MeetingUsers.Add(meetingUser);
            await _context.SaveChangesAsync();
            return true;
        }

        // Удаление участника (только организатор, нельзя удалить себя)
        public async Task<bool> RemoveParticipantAsync(Guid meetingId, Guid organizerId, MeetingRemoveParticipantDto dto)
        {
            // Проверяем, что organizerId действительно организатор этой встречи
            if (!await IsOrganizerAsync(meetingId, organizerId))
                return false;

            // Нельзя удалить самого организатора
            if (dto.UserId == organizerId)
                return false;

            var meetingUser = await _context.MeetingUsers
                .FirstOrDefaultAsync(mu => mu.MeetingId == meetingId && mu.UserId == dto.UserId && mu.Role == 0);
            if (meetingUser == null)
                return false;

            _context.MeetingUsers.Remove(meetingUser);
            await _context.SaveChangesAsync();
            return true;
        }

        // Получение всех встреч пользователя (организатор или участник)
        public async Task<List<MeetingShortDto>> GetUserMeetingsAsync(Guid userId)
        {
            var meetings = await _context.MeetingUsers
                .Where(mu => mu.UserId == userId)
                .Include(mu => mu.Meeting)
                .ThenInclude(m => m.MeetingUsers)
                .ThenInclude(mu => mu.User)
                .ToListAsync();

            var result = new List<MeetingShortDto>();
            foreach (var mu in meetings)
            {
                var meeting = mu.Meeting;
                var organizerMU = meeting.MeetingUsers.FirstOrDefault(x => x.Role == 1);
                var organizerUser = organizerMU?.User;
                result.Add(new MeetingShortDto
                {
                    MeetingId = meeting.MeetingId,
                    Title = meeting.Title,
                    MeetingDate = meeting.MeetingDate,
                    MeetingTime = meeting.MeetingTime,
                    Location = meeting.Location,
                    Link = meeting.Link,
                    OrganizerFullName = organizerUser != null
                        ? $"{organizerUser.Surname} {organizerUser.Name}"
                        : ""
                });
            }

            // Убираем дубли если пользователь вдруг был участником и организатором (теоретически не должно быть)
            return result.GroupBy(x => x.MeetingId).Select(g => g.First()).OrderBy(x => x.MeetingDate).ThenBy(x => x.MeetingTime).ToList();
        }

        // Получение подробной информации о встрече (только для участников и организатора)
        public async Task<MeetingDetailDto?> GetMeetingByIdAsync(Guid meetingId, Guid userId)
        {
            // Проверка доступа
            if (!await IsParticipantOrOrganizerAsync(meetingId, userId))
                return null;

            var meeting = await _context.Meetings
                .Include(m => m.MeetingUsers)
                    .ThenInclude(mu => mu.User)
                .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

            if (meeting == null)
                return null;

            var organizerMU = meeting.MeetingUsers.FirstOrDefault(mu => mu.Role == 1);
            var organizer = organizerMU != null ? new MeetingParticipantDto
            {
                UserId = organizerMU.UserId,
                FullName = $"{organizerMU.User.Surname} {organizerMU.User.Name}",
                Role = 1
            } : null;

            var participants = meeting.MeetingUsers
                .Where(mu => mu.Role == 0)
                .Select(mu => new MeetingParticipantDto
                {
                    UserId = mu.UserId,
                    FullName = $"{mu.User.Surname} {mu.User.Name}",
                    Role = 0
                })
                .ToList();

            return new MeetingDetailDto
            {
                MeetingId = meeting.MeetingId,
                Title = meeting.Title,
                Location = meeting.Location,
                Link = meeting.Link,
                MeetingDate = meeting.MeetingDate,
                MeetingTime = meeting.MeetingTime,
                Organizer = organizer,
                Participants = participants
            };
        }
    }
}