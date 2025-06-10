namespace StudetCouncilPlannerAPI.Models.DTOs
{
    public class UserRegisterDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string? Patronymic { get; set; }
        public string Group { get; set; }
        public long Phone { get; set; }
        public string Contacts { get; set; }
    }

    public class UserLoginDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Login { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string? Patronymic { get; set; }
        public string Group { get; set; }
        public long Phone { get; set; }
        public string Contacts { get; set; }
        public short Role { get; set; }
    }

    public class UserLoginResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }
}