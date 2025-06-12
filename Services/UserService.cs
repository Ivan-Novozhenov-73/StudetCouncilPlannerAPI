using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Interfaces;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Services
{
    public class UserService(ApplicationDbContext dbContext) : IUserService
    {
        // Получить список пользователей с фильтрацией и пагинацией
        public async Task<List<UserDto>> GetUsersAsync(UserListQueryDto query)
        {
            var usersQuery = dbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Surname.Contains(query.Search) ||
                    u.Name.Contains(query.Search) ||
                    (u.Patronymic != null && u.Patronymic.Contains(query.Search)) ||
                    u.Group.Contains(query.Search)
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Group))
                usersQuery = usersQuery.Where(u => u.Group == query.Group);

            if (query.Role.HasValue)
                usersQuery = usersQuery.Where(u => u.Role == query.Role);

            if (query.Archive.HasValue)
                usersQuery = usersQuery.Where(u => u.Archive == query.Archive);

            var skip = (query.Page - 1) * query.PageSize;

            var users = await usersQuery
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            return users.Select(ToUserDto).ToList();
        }

        // Получить пользователя по id
        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await dbContext.Users.FindAsync(userId);
            return user != null ? ToUserDto(user) : null;
        }

        // Обновить пользователя
        public async Task<bool> UpdateUserAsync(Guid userId, UserUpdateDto dto, User currentUser)
        {
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Роль 0 и 1 могут менять только себя, 2 — всех
            if (currentUser.Role < 2 && currentUser.UserId != userId)
                return false;

            user.Surname = dto.Surname ?? user.Surname;
            user.Name = dto.Name ?? user.Name;
            user.Patronymic = dto.Patronymic ?? user.Patronymic;
            user.Group = dto.Group ?? user.Group;
            user.Phone = dto.Phone ?? user.Phone;
            user.Contacts = dto.Contacts ?? user.Contacts;

            await dbContext.SaveChangesAsync();
            return true;
        }

        // Архивировать пользователя (роль 2)
        public async Task<bool> ArchiveUserAsync(Guid userId, User currentUser)
        {
            if (currentUser.Role != 2)
                return false;

            if (currentUser.UserId == userId)
            {
                // Проверка: нельзя архивировать себя, если ты единственный неархивированный админ
                int activeAdmins = await dbContext.Users.CountAsync(u => u.Role == 2 && !u.Archive);
                if (activeAdmins <= 1)
                    return false;
            }

            var user = await dbContext.Users.FindAsync(userId);
            if (user == null || user.Archive)
                return false;

            user.Archive = true;
            await dbContext.SaveChangesAsync();
            return true;
        }

        // Восстановить пользователя из архива (роль 2)
        public async Task<bool> RestoreUserAsync(Guid userId, User currentUser)
        {
            if (currentUser.Role != 2)
                return false;

            var user = await dbContext.Users.FindAsync(userId);
            if (user == null || !user.Archive)
                return false;

            user.Archive = false;
            await dbContext.SaveChangesAsync();
            return true;
        }

        // Изменить роль пользователя (роль 2, с защитой от потери последнего админа)
        public async Task<bool> ChangeUserRoleAsync(Guid userId, short newRole, User currentUser)
        {
            if (currentUser.Role != 2)
                return false;

            var user = await dbContext.Users.FindAsync(userId);
            if (user == null || user.Role == newRole)
                return false;

            // Защита: нельзя понизить себя, если ты последний админ
            if (user.UserId == currentUser.UserId && user.Role == 2 && newRole != 2)
            {
                int activeAdmins = await dbContext.Users.CountAsync(u => u.Role == 2 && !u.Archive);
                if (activeAdmins <= 1)
                    return false;
            }

            user.Role = newRole;
            await dbContext.SaveChangesAsync();
            return true;
        }

        // Маппинг User -> UserDto
        private static UserDto ToUserDto(User u) => new UserDto
        {
            UserId = u.UserId,
            Login = u.Login,
            Surname = u.Surname,
            Name = u.Name,
            Patronymic = u.Patronymic,
            Role = u.Role,
            Group = u.Group,
            Phone = u.Phone,
            Contacts = u.Contacts,
            Archive = u.Archive
        };
    }
}