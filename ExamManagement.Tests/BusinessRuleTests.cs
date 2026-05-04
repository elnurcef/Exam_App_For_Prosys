using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExamManagement.Tests;

public sealed class BusinessRuleTests
{
    [Fact]
    public async Task TeacherSignup_WithDuplicateEmail_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var authService = CreateAuthService(dbContext);

        var request = SignupRequest("teacher@example.com");
        await authService.SignupAsync(request, default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => authService.SignupAsync(request, default));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var authService = CreateAuthService(dbContext);

        await authService.SignupAsync(SignupRequest("teacher@example.com"), default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => authService.LoginAsync(new LoginRequestDto
        {
            Email = "teacher@example.com",
            Password = "WrongPassword1"
        }, default));

        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task CreateLesson_WithInvalidCode_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var lessonService = new LessonService(dbContext);

        var exception = await Assert.ThrowsAsync<ApiException>(() => lessonService.CreateAsync(teacher.Id, new LessonCreateDto
        {
            Code = "AB",
            Name = "Math",
            ClassLevel = 8
        }, default));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task CreateLesson_WithDuplicateCodeForSameTeacher_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var lessonService = new LessonService(dbContext);

        await lessonService.CreateAsync(teacher.Id, LessonRequest("mat"), default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => lessonService.CreateAsync(teacher.Id, LessonRequest("MAT"), default));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithDuplicateNumberForSameTeacher_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var studentService = new StudentService(dbContext);

        await studentService.CreateAsync(teacher.Id, StudentRequest(1001, 8), default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => studentService.CreateAsync(teacher.Id, StudentRequest(1001, 8), default));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task ScheduleExam_WithMismatchedClass_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var lessonService = new LessonService(dbContext);
        var studentService = new StudentService(dbContext);
        var examService = new ExamService(dbContext);

        var lesson = await lessonService.CreateAsync(teacher.Id, LessonRequest("MAT", 8), default);
        var student = await studentService.CreateAsync(teacher.Id, StudentRequest(1001, 9), default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => examService.ScheduleAsync(teacher.Id, new ExamScheduleDto
        {
            LessonId = lesson.Id,
            StudentId = student.Id,
            ExamDate = new DateOnly(2026, 5, 4)
        }, default));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task ScheduleExam_WithDuplicateActiveExam_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var (lesson, student) = await CreateMatchingLessonAndStudentAsync(dbContext, teacher.Id);
        var examService = new ExamService(dbContext);
        var request = ExamRequest(lesson.Id, student.Id);

        await examService.ScheduleAsync(teacher.Id, request, default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => examService.ScheduleAsync(teacher.Id, request, default));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task GradeCancelledExam_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var teacher = await AddTeacherAsync(dbContext, "teacher@example.com");
        var (lesson, student) = await CreateMatchingLessonAndStudentAsync(dbContext, teacher.Id);
        var examService = new ExamService(dbContext);

        var exam = await examService.ScheduleAsync(teacher.Id, ExamRequest(lesson.Id, student.Id), default);
        await examService.CancelAsync(teacher.Id, exam.Id, default);

        var exception = await Assert.ThrowsAsync<ApiException>(() => examService.GradeAsync(teacher.Id, exam.Id, new ExamGradeDto
        {
            Grade = 5
        }, default));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task TeacherCannotAccessAnotherTeachersLessonStudentOrExam()
    {
        await using var dbContext = CreateDbContext();
        var owner = await AddTeacherAsync(dbContext, "owner@example.com");
        var other = await AddTeacherAsync(dbContext, "other@example.com");
        var lessonService = new LessonService(dbContext);
        var studentService = new StudentService(dbContext);
        var examService = new ExamService(dbContext);
        var lesson = await lessonService.CreateAsync(owner.Id, LessonRequest("MAT", 8), default);
        var student = await studentService.CreateAsync(owner.Id, StudentRequest(1001, 8), default);
        var exam = await examService.ScheduleAsync(owner.Id, ExamRequest(lesson.Id, student.Id), default);

        var lessonException = await Assert.ThrowsAsync<ApiException>(() => lessonService.GetByIdAsync(other.Id, lesson.Id, default));
        var studentException = await Assert.ThrowsAsync<ApiException>(() => studentService.GetByIdAsync(other.Id, student.Id, default));
        var examException = await Assert.ThrowsAsync<ApiException>(() => examService.GetByIdAsync(other.Id, exam.Id, default));

        Assert.Equal(StatusCodes.Status403Forbidden, lessonException.StatusCode);
        Assert.Equal(StatusCodes.Status403Forbidden, studentException.StatusCode);
        Assert.Equal(StatusCodes.Status403Forbidden, examException.StatusCode);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IAuthService CreateAuthService(AppDbContext dbContext)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "Tests",
            Audience = "Tests",
            Secret = "UNIT_TEST_SECRET_KEY_32_BYTES_MINIMUM",
            ExpirationMinutes = 30
        });

        return new AuthService(dbContext, new PasswordHasher<Teacher>(), new JwtTokenService(jwtOptions));
    }

    private static async Task<Teacher> AddTeacherAsync(AppDbContext dbContext, string email)
    {
        var teacher = new Teacher
        {
            FirstName = "Test",
            LastName = "Teacher",
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        teacher.PasswordHash = new PasswordHasher<Teacher>().HashPassword(teacher, "Password1");

        dbContext.Teachers.Add(teacher);
        await dbContext.SaveChangesAsync();
        return teacher;
    }

    private static SignupRequestDto SignupRequest(string email) => new()
    {
        FirstName = "Test",
        LastName = "Teacher",
        Email = email,
        Password = "Password1",
        ConfirmPassword = "Password1"
    };

    private static LessonCreateDto LessonRequest(string code, int classLevel = 8) => new()
    {
        Code = code,
        Name = "Mathematics",
        ClassLevel = classLevel
    };

    private static StudentCreateDto StudentRequest(int number, int classLevel) => new()
    {
        Number = number,
        FirstName = "Student",
        LastName = "One",
        ClassLevel = classLevel
    };

    private static ExamScheduleDto ExamRequest(int lessonId, int studentId) => new()
    {
        LessonId = lessonId,
        StudentId = studentId,
        ExamDate = new DateOnly(2026, 5, 4)
    };

    private static async Task<(LessonDto Lesson, StudentDto Student)> CreateMatchingLessonAndStudentAsync(AppDbContext dbContext, int teacherId)
    {
        var lessonService = new LessonService(dbContext);
        var studentService = new StudentService(dbContext);
        var lesson = await lessonService.CreateAsync(teacherId, LessonRequest("MAT", 8), default);
        var student = await studentService.CreateAsync(teacherId, StudentRequest(1001, 8), default);
        return (lesson, student);
    }
}
