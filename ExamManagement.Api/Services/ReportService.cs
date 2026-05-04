using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Services;

public sealed class ReportService(AppDbContext dbContext) : IReportService
{
    public async Task<IReadOnlyList<ExamResultDto>> GetExamResultsAsync(int teacherId, ExamFilterDto filter, CancellationToken cancellationToken)
    {
        ValidateStatus(filter.Status);

        return await ApplyFilters(BaseReportQuery(teacherId), filter)
            .OrderByDescending(e => e.ExamDate)
            .ThenBy(e => e.Student.Number)
            .Select(e => e.ToResultDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResultDto>> GetStudentReportAsync(int teacherId, int studentId, ExamFilterDto filter, CancellationToken cancellationToken)
    {
        await EnsureStudentOwnershipAsync(teacherId, studentId, cancellationToken);
        filter.StudentId = studentId;
        return await GetExamResultsAsync(teacherId, filter, cancellationToken);
    }

    public async Task<IReadOnlyList<ExamResultDto>> GetLessonReportAsync(int teacherId, int lessonId, ExamFilterDto filter, CancellationToken cancellationToken)
    {
        await EnsureLessonOwnershipAsync(teacherId, lessonId, cancellationToken);
        filter.LessonId = lessonId;
        return await GetExamResultsAsync(teacherId, filter, cancellationToken);
    }

    private IQueryable<Exam> BaseReportQuery(int teacherId)
    {
        return dbContext.Exams
            .AsNoTracking()
            .Include(e => e.Lesson)
            .Include(e => e.Student)
            .Where(e => e.TeacherId == teacherId);
    }

    private static IQueryable<Exam> ApplyFilters(IQueryable<Exam> query, ExamFilterDto filter)
    {
        if (filter.LessonId.HasValue)
        {
            query = query.Where(e => e.LessonId == filter.LessonId.Value);
        }

        if (filter.StudentId.HasValue)
        {
            query = query.Where(e => e.StudentId == filter.StudentId.Value);
        }

        if (filter.ClassLevel.HasValue)
        {
            query = query.Where(e => e.Student.ClassLevel == filter.ClassLevel.Value);
        }

        if (filter.ExamDate.HasValue)
        {
            query = query.Where(e => e.ExamDate == filter.ExamDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(e => e.Status == filter.Status);
        }

        return query;
    }

    private async Task EnsureStudentOwnershipAsync(int teacherId, int studentId, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw ApiException.NotFound("Student was not found.");

        if (student.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }
    }

    private async Task EnsureLessonOwnershipAsync(int teacherId, int lessonId, CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken)
            ?? throw ApiException.NotFound("Lesson was not found.");

        if (lesson.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }
    }

    private static void ValidateStatus(string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && !ExamStatus.IsValid(status))
        {
            throw ApiException.BadRequest("Exam status is invalid.");
        }
    }
}
