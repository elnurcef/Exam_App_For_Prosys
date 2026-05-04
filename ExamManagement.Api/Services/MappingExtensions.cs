using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;

namespace ExamManagement.Api.Services;

internal static class MappingExtensions
{
    public static TeacherDto ToDto(this Teacher teacher) => new()
    {
        Id = teacher.Id,
        FirstName = teacher.FirstName,
        LastName = teacher.LastName,
        Email = teacher.Email,
        CreatedAt = teacher.CreatedAt,
        UpdatedAt = teacher.UpdatedAt
    };

    public static LessonDto ToDto(this Lesson lesson) => new()
    {
        Id = lesson.Id,
        Code = lesson.Code,
        Name = lesson.Name,
        ClassLevel = lesson.ClassLevel,
        TeacherFirstName = lesson.TeacherFirstName,
        TeacherLastName = lesson.TeacherLastName,
        CreatedAt = lesson.CreatedAt,
        UpdatedAt = lesson.UpdatedAt
    };

    public static StudentDto ToDto(this Student student) => new()
    {
        Id = student.Id,
        Number = student.Number,
        FirstName = student.FirstName,
        LastName = student.LastName,
        ClassLevel = student.ClassLevel,
        CreatedAt = student.CreatedAt,
        UpdatedAt = student.UpdatedAt
    };

    public static ExamDto ToDto(this Exam exam) => new()
    {
        Id = exam.Id,
        LessonId = exam.LessonId,
        LessonCode = exam.Lesson.Code,
        LessonName = exam.Lesson.Name,
        StudentId = exam.StudentId,
        StudentNumber = exam.Student.Number,
        StudentFullName = $"{exam.Student.FirstName} {exam.Student.LastName}",
        ClassLevel = exam.Student.ClassLevel,
        ExamDate = exam.ExamDate,
        Grade = exam.Grade,
        Status = exam.Status,
        CreatedAt = exam.CreatedAt,
        UpdatedAt = exam.UpdatedAt
    };

    public static ExamResultDto ToResultDto(this Exam exam) => new()
    {
        ExamId = exam.Id,
        StudentId = exam.StudentId,
        StudentNumber = exam.Student.Number,
        StudentFullName = $"{exam.Student.FirstName} {exam.Student.LastName}",
        StudentClass = exam.Student.ClassLevel,
        LessonId = exam.LessonId,
        LessonCode = exam.Lesson.Code,
        LessonName = exam.Lesson.Name,
        ExamDate = exam.ExamDate,
        Grade = exam.Grade,
        Status = exam.Status
    };
}
