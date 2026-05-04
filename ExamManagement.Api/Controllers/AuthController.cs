using ExamManagement.Api.DTOs;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup(SignupRequestDto request, CancellationToken cancellationToken)
    {
        var response = await authService.SignupAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Me), null, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<TeacherDto>> Me(CancellationToken cancellationToken)
    {
        return Ok(await authService.GetMeAsync(User.GetTeacherId(), cancellationToken));
    }
}
