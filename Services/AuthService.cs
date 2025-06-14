using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Interfaces;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<UserDto> RegisterAsync(UserRegisterDto dto)
        {
            if (await context.Users.AnyAsync(x => x.Login == dto.Login))
            {
                return null!;
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Surname = dto.Surname,
                Name = dto.Name,
                Patronymic = dto.Patronymic,
                Group = dto.Group,
                Phone = dto.Phone,
                Contacts = dto.Contacts,
                Role = 0 // или твой дефолтный роль
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Login == dto.Login);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null!;
            }

            return new UserLoginResponseDto
            {
                Token = GenerateJwtToken(user),
                User = MapToDto(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Login),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            return new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(12),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256)
                    )
                );
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Login = user.Login,
                Surname = user.Surname,
                Name = user.Name,
                Patronymic = user.Patronymic,
                Group = user.Group,
                Phone = user.Phone,
                Contacts = user.Contacts,
                Role = user.Role
            };
        }
    }
}