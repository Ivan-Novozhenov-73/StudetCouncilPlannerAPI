using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync(UserListQueryDto query);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> UpdateUserAsync(Guid userId, UserUpdateDto dto, User currentUser);
    Task<bool> ArchiveUserAsync(Guid userId, User currentUser);
    Task<bool> RestoreUserAsync(Guid userId, User currentUser);
    Task<bool> ChangeUserRoleAsync(Guid userId, short newRole, User currentUser);
}