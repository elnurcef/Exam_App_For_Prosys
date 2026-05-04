using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Services;

public sealed class AuthService(
    AppDbContext dbContext,
    IPasswordHasher<Teacher> passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponseDto> SignupAsync(SignupRequestDto request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (await dbContext.Teachers.AnyAsync(t => t.Email == email, cancellationToken))
        {
            throw ApiException.Conflict("A teacher with this email already exists.");
        }

        var teacher = new Teacher
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        teacher.PasswordHash = passwordHasher.HashPassword(teacher, request.Password);

        dbContext.Teachers.Add(teacher);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(teacher);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var teacher = await dbContext.Teachers.SingleOrDefaultAsync(t => t.Email == email, cancellationToken);
        if (teacher is null)
        {
            throw ApiException.Unauthorized("Invalid email or password.");
        }

        var result = passwordHasher.VerifyHashedPassword(teacher, teacher.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw ApiException.Unauthorized("Invalid email or password.");
        }

        return CreateAuthResponse(teacher);
    }

    public async Task<TeacherDto> GetMeAsync(int teacherId, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers.FindAsync([teacherId], cancellationToken)
            ?? throw ApiException.NotFound("Teacher profile was not found.");

        return teacher.ToDto();
    }

    private AuthResponseDto CreateAuthResponse(Teacher teacher)
    {
        var (token, expiresAt) = jwtTokenService.CreateToken(teacher);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Teacher = teacher.ToDto()
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
