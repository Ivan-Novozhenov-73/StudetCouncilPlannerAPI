using StudetCouncilPlannerAPI.Models.DTOs;

namespace StudetCouncilPlannerAPI.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(UserRegisterDto dto);
    Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);
}