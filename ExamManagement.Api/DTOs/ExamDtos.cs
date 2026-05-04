using System.ComponentModel.DataAnnotations;

namespace ExamManagement.Api.DTOs;

public sealed class ExamFilterDto
{
    public int? LessonId { get; set; }
    public int? StudentId { get; set; }

    [Range(1, 12, ErrorMessage = "Class level must be between 1 and 12.")]
    public int? ClassLevel { get; set; }

    public DateOnly? ExamDate { get; set; }
    public string? Status { get; set; }
}

public sealed class ExamScheduleDto
{
    [Required(ErrorMessage = "Lesson is required.")]
    public int? LessonId { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "Exam date is required.")]
    public DateOnly? ExamDate { get; set; }
}

public sealed class ExamUpdateDto
{
    [Required(ErrorMessage = "Lesson is required.")]
    public int? LessonId { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "Exam date is required.")]
    public DateOnly? ExamDate { get; set; }
}

public sealed class ExamGradeDto
{
    [Required(ErrorMessage = "Grade is required.")]
    [Range(0, 5, ErrorMessage = "Grade must be between 0 and 5.")]
    public int? Grade { get; set; }
}

public sealed class ExamDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string LessonCode { get; set; } = string.Empty;
    public string LessonName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public int StudentNumber { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public int ClassLevel { get; set; }
    public DateOnly ExamDate { get; set; }
    public int? Grade { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
