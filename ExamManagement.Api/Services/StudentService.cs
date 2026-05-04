using ExamManagement.Api.Data;
using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;
using ExamManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Services;

public sealed class StudentService(AppDbContext dbContext) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(int teacherId, CancellationToken cancellationToken)
    {
        return await dbContext.Students
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId)
            .OrderBy(s => s.ClassLevel)
            .ThenBy(s => s.Number)
            .Select(s => s.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var student = await GetStudentWithOwnershipAsync(teacherId, id, cancellationToken);
        return student.ToDto();
    }

    public async Task<StudentDto> CreateAsync(int teacherId, StudentCreateDto request, CancellationToken cancellationToken)
    {
        if (await dbContext.Students.AnyAsync(s => s.TeacherId == teacherId && s.Number == request.Number, cancellationToken))
        {
            throw ApiException.Conflict("A student with this number already exists.");
        }

        var student = new Student
        {
            Number = request.Number,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            ClassLevel = (byte)request.ClassLevel,
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync(cancellationToken);
        return student.ToDto();
    }

    public async Task<StudentDto> UpdateAsync(int teacherId, int id, StudentUpdateDto request, CancellationToken cancellationToken)
    {
        var student = await GetStudentWithOwnershipAsync(teacherId, id, cancellationToken);

        if (await dbContext.Students.AnyAsync(s => s.TeacherId == teacherId && s.Id != id && s.Number == request.Number, cancellationToken))
        {
            throw ApiException.Conflict("A student with this number already exists.");
        }

        if (student.ClassLevel != request.ClassLevel
            && await dbContext.Exams.AnyAsync(e => e.StudentId == id && e.Status != ExamStatus.Cancelled, cancellationToken))
        {
            throw ApiException.BadRequest("Student class cannot be changed while active exams are connected to the student.");
        }

        student.Number = request.Number;
        student.FirstName = request.FirstName.Trim();
        student.LastName = request.LastName.Trim();
        student.ClassLevel = (byte)request.ClassLevel;
        student.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return student.ToDto();
    }

    public async Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var student = await GetStudentWithOwnershipAsync(teacherId, id, cancellationToken);

        if (await dbContext.Exams.AnyAsync(e => e.StudentId == id, cancellationToken))
        {
            throw ApiException.BadRequest("Student cannot be deleted because the student has exam records. Keep the student for historical results.");
        }

        dbContext.Students.Remove(student);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Student> GetStudentWithOwnershipAsync(int teacherId, int id, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("Student was not found.");

        if (student.TeacherId != teacherId)
        {
            throw ApiException.Forbidden("You are not allowed to access this resource.");
        }

        return student;
    }
}
