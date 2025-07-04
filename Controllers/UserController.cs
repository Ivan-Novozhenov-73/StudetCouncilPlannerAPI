using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;
using System.Security.Claims;
using StudetCouncilPlannerAPI.Interfaces;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        // Вспомогательный метод: получить текущего пользователя из токена/контекста
        private User GetCurrentUser()
        {
            // Здесь пример: получаем Id и Role из Claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleStr = User.FindFirstValue(ClaimTypes.Role);
            var group = User.FindFirstValue("Group") ?? "";
            var login = User.Identity?.Name ?? "";
            var surname = User.FindFirstValue("Surname") ?? "";
            var name = User.FindFirstValue("Name") ?? "";
            var patronymic = User.FindFirstValue("Patronymic");
            var phone = User.FindFirstValue("Phone") ?? "0";
            var contacts = User.FindFirstValue("Contacts") ?? "";

            return new User
            {
                UserId = Guid.TryParse(userIdStr, out var guid) ? guid : Guid.Empty,
                Role = short.TryParse(roleStr, out var role) ? role : (short)0,
                Group = group,
                Login = login,
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                Phone = long.TryParse(phone, out var ph) ? ph : 0,
                Contacts = contacts
            };
        }

        /// <summary>
        /// Получить список пользователей с фильтрацией и пагинацией
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] UserListQueryDto query)
        {
            var users = await userService.GetUsersAsync(query);
            return Ok(users);
        }

        /// <summary>
        /// Получить пользователя по id
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUserById([FromRoute] Guid id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UserUpdateDto dto)
        {
            var currentUser = GetCurrentUser();
            var result = await userService.UpdateUserAsync(id, dto, currentUser);
            if (!result)
                return Forbid();
            return NoContent();
        }

        /// <summary>
        /// Архивировать пользователя (только для роли 2)
        /// </summary>
        [HttpPost("{id}/archive")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> ArchiveUser([FromRoute] Guid id)
        {
            var currentUser = GetCurrentUser();
            var result = await userService.ArchiveUserAsync(id, currentUser);
            if (!result)
                return Forbid();
            return NoContent();
        }

        /// <summary>
        /// Восстановить пользователя из архива (только для роли 2)
        /// </summary>
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RestoreUser([FromRoute] Guid id)
        {
            var currentUser = GetCurrentUser();
            var result = await userService.RestoreUserAsync(id, currentUser);
            if (!result)
                return Forbid();
            return NoContent();
        }

        /// <summary>
        /// Изменить роль пользователя (только для роли 2)
        /// </summary>
        [HttpPost("{id}/changerole")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> ChangeUserRole([FromRoute] Guid id, [FromBody] UserChangeRoleDto dto)
        {
            var currentUser = GetCurrentUser();
            var result = await userService.ChangeUserRoleAsync(id, dto.NewRole, currentUser);
            if (!result)
                return Forbid();
            return NoContent();
        }
    }
}