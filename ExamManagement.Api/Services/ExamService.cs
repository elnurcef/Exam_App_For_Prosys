using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Services;

public sealed class ExamService(AppDbContext dbContext) : IExamService
{
    public async Task<IReadOnlyList<ExamDto>> GetAllAsync(int teacherId, ExamFilterDto filter, CancellationToken cancellationToken)
    {
        ValidateStatus(filter.Status);

        return await ApplyFilters(BaseExamQuery(teacherId), filter)
            .OrderByDescending(e => e.ExamDate)
            .ThenBy(e => e.Student.Number)
            .Select(e => e.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<ExamDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var exam = await GetExamWithOwnershipAsync(teacherId, id, cancellationToken);
        return exam.ToDto();
    }

    public async Task<ExamDto> ScheduleAsync(int teacherId, ExamScheduleDto request, CancellationToken cancellationToken)
    {
        var lesson = await GetLessonOwnedByTeacherAsync(teacherId, request.LessonId!.Value, cancellationToken);
        var student = await GetStudentOwnedByTeacherAsync(teacherId, request.StudentId!.Value, cancellationToken);
        var examDate = request.ExamDate!.Value;

        await ValidateScheduleAsync(teacherId, lesson, student, examDate, null, cancellationToken);

        var exam = new Exam
        {
            LessonId = lesson.Id,
            StudentId = student.Id,
            ExamDate = examDate,
            Grade = null,
            Status = ExamStatus.Scheduled,
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Exams.Add(exam);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (await BaseExamQuery(teacherId).SingleAsync(e => e.Id == exam.Id, cancellationToken)).ToDto();
    }

    public async Task<ExamDto> UpdateAsync(int teacherId, int id, ExamUpdateDto request, CancellationToken cancellationToken)
    {
        var exam = await GetExamWithOwnershipAsync(teacherId, id, cancellationToken);
        if (exam.Status == ExamStatus.Cancelled)
        {
            throw ApiException.BadRequest("Cancelled exam cannot be edited.");
        }

        var lesson = await GetLessonOwnedByTeacherAsync(teacherId, request.LessonId!.Value, cancellationToken);
        var student = await GetStudentOwnedByTeacherAsync(teacherId, request.StudentId!.Value, cancellationToken);
        var examDate = request.ExamDate!.Value;

        await ValidateScheduleAsync(teacherId, lesson, student, examDate, id, cancellationToken);

        exam.LessonId = lesson.Id;
        exam.StudentId = student.Id;
        exam.ExamDate = examDate;
        exam.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return exam.ToDto();
    }

    public async Task<ExamDto> GradeAsync(int teacherId, int id, ExamGradeDto request, CancellationToken cancellationToken)
    {
        var exam = await GetExamWithOwnershipAsync(teacherId, id, cancellationToken);
        if (exam.Status == ExamStatus.Cancelled)
        {
            throw ApiException.BadRequest("Cancelled exam cannot be graded.");
        }

        var grade = request.Grade!.Value;
        if (grade is < 0 or > 5)
        {
            throw ApiException.BadRequest("Grade must be between 0 and 5.");
        }

        exam.Grade = (byte)grade;
        exam.Status = ExamStatus.Graded;
        exam.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return exam.ToDto();
    }

    public async Task<ExamDto> CancelAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var exam = await GetExamWithOwnershipAsync(teacherId, id, cancellationToken);
        if (exam.Status == ExamStatus.Cancelled)
        {
            return exam.ToDto();
        }

        exam.Status = ExamStatus.Cancelled;
        exam.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return exam.ToDto();
    }

    public async Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var exam = await GetExamWithOwnershipAsync(teacherId, id, cancellationToken);
        if (exam.Status != ExamStatus.Cancelled)
        {
            throw ApiException.BadRequest("Active exams cannot be hard deleted. Cancel the exam to preserve history.");
        }

        dbContext.Exams.Remove(exam);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Exam> BaseExamQuery(int teacherId)
    {
        return dbContext.Exams
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

    private async Task<Exam> GetExamWithOwnershipAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var exam = await dbContext.Exams
            .Include(e => e.Lesson)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("Exam was not found.");

        if (exam.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }

        return exam;
    }

    private async Task<Lesson> GetLessonOwnedByTeacherAsync(int teacherId, int lessonId, CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken)
            ?? throw ApiException.NotFound("Lesson was not found.");

        if (lesson.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }

        return lesson;
    }

    private async Task<Student> GetStudentOwnedByTeacherAsync(int teacherId, int studentId, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw ApiException.NotFound("Student was not found.");

        if (student.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }

        return student;
    }

    private async Task ValidateScheduleAsync(
        int teacherId,
        Lesson lesson,
        Student student,
        DateOnly examDate,
        int? currentExamId,
        CancellationToken cancellationToken)
    {
        if (lesson.ClassLevel != student.ClassLevel)
        {
            throw ApiException.BadRequest("Student class must match lesson class.");
        }

        var duplicateExists = await dbContext.Exams.AnyAsync(e =>
            e.TeacherId == teacherId
            && e.LessonId == lesson.Id
            && e.StudentId == student.Id
            && e.ExamDate == examDate
            && e.Status != ExamStatus.Cancelled
            && (!currentExamId.HasValue || e.Id != currentExamId.Value),
            cancellationToken);

        if (duplicateExists)
        {
            throw ApiException.Conflict("An active exam already exists for this student, lesson, and date.");
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
