using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto) =>
            Ok(await authService.RegisterAsync(dto));

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto) => 
            Ok(await authService.LoginAsync(dto));
    }
}