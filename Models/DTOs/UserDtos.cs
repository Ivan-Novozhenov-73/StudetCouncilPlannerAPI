namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // DTO для регистрации
    public class UserRegisterDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string Group { get; set; } = string.Empty;
        public long Phone { get; set; }
        public string Contacts { get; set; } = string.Empty;
    }

    // DTO для аутентификации
    public class UserLoginDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO для выдачи информации о пользователе
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string Group { get; set; } = string.Empty;
        public long Phone { get; set; }
        public string Contacts { get; set; } = string.Empty;
        public short Role { get; set; }
        public bool Archive { get; set; }
    }

    // DTO для выдачи ответа при аутентификации
    public class UserLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }

    // DTO для параметров фильтрации и поиска пользователей в списке
    public class UserListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; } // ФИО, группа, роль (общий текстовый поиск)
        public string? Group { get; set; }
        public short? Role { get; set; }
        public bool? Archive { get; set; }
    }

    // DTO для обновления информации о пользователе
    public class UserUpdateDto
    {
        public string? Surname { get; set; }
        public string? Name { get; set; }
        public string? Patronymic { get; set; }
        public string? Group { get; set; }
        public long? Phone { get; set; }
        public string? Contacts { get; set; }
        // поле Role менять только через отдельный endpoint!
    }

    // DTO для изменения роли пользователя
    public class UserChangeRoleDto
    {
        public short NewRole { get; set; }
    }
}