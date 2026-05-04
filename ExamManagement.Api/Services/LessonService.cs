using System.Text.RegularExpressions;
using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Services;

public sealed partial class LessonService(AppDbContext dbContext) : ILessonService
{
    public async Task<IReadOnlyList<LessonDto>> GetAllAsync(int teacherId, CancellationToken cancellationToken)
    {
        return await dbContext.Lessons
            .AsNoTracking()
            .Where(l => l.TeacherId == teacherId)
            .OrderBy(l => l.ClassLevel)
            .ThenBy(l => l.Code)
            .Select(l => l.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<LessonDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var lesson = await GetLessonWithOwnershipAsync(teacherId, id, cancellationToken);
        return lesson.ToDto();
    }

    public async Task<LessonDto> CreateAsync(int teacherId, LessonCreateDto request, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers.FindAsync([teacherId], cancellationToken)
            ?? throw ApiException.NotFound("Teacher profile was not found.");

        var code = NormalizeCode(request.Code);
        ValidateCode(code);

        if (await dbContext.Lessons.AnyAsync(l => l.TeacherId == teacherId && l.Code == code, cancellationToken))
        {
            throw ApiException.Conflict("A lesson with this code already exists.");
        }

        var lesson = new Lesson
        {
            Code = code,
            Name = request.Name.Trim(),
            ClassLevel = (byte)request.ClassLevel,
            TeacherFirstName = teacher.FirstName,
            TeacherLastName = teacher.LastName,
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync(cancellationToken);
        return lesson.ToDto();
    }

    public async Task<LessonDto> UpdateAsync(int teacherId, int id, LessonUpdateDto request, CancellationToken cancellationToken)
    {
        var lesson = await GetLessonWithOwnershipAsync(teacherId, id, cancellationToken);
        var code = NormalizeCode(request.Code);
        ValidateCode(code);

        if (await dbContext.Lessons.AnyAsync(l => l.TeacherId == teacherId && l.Id != id && l.Code == code, cancellationToken))
        {
            throw ApiException.Conflict("A lesson with this code already exists.");
        }

        if (lesson.ClassLevel != request.ClassLevel
            && await dbContext.Exams.AnyAsync(e => e.LessonId == id && e.Status != ExamStatus.Cancelled, cancellationToken))
        {
            throw ApiException.BadRequest("Lesson class cannot be changed while active exams are connected to it.");
        }

        lesson.Code = code;
        lesson.Name = request.Name.Trim();
        lesson.ClassLevel = (byte)request.ClassLevel;
        lesson.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return lesson.ToDto();
    }

    public async Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var lesson = await GetLessonWithOwnershipAsync(teacherId, id, cancellationToken);

        if (await dbContext.Exams.AnyAsync(e => e.LessonId == id, cancellationToken))
        {
            throw ApiException.BadRequest("Lesson cannot be deleted because it has exam records. Keep it for historical results.");
        }

        dbContext.Lessons.Remove(lesson);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Lesson> GetLessonWithOwnershipAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("Lesson was not found.");

        if (lesson.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }

        return lesson;
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static void ValidateCode(string code)
    {
        if (code.Length != 3)
        {
            throw ApiException.BadRequest("Lesson code must be exactly 3 characters.");
        }

        if (!LessonCodeRegex().IsMatch(code))
        {
            throw ApiException.BadRequest("Lesson code must contain only uppercase letters and digits.");
        }
    }

    [GeneratedRegex("^[A-Z0-9]{3}$")]
    private static partial Regex LessonCodeRegex();
}
