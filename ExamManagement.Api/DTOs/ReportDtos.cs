namespace ExamManagement.Api.DTOs;

public sealed class ExamResultDto
{
    public int ExamId { get; set; }
    public int StudentId { get; set; }
    public int StudentNumber { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public int StudentClass { get; set; }
    public int LessonId { get; set; }
    public string LessonCode { get; set; } = string.Empty;
    public string LessonName { get; set; } = string.Empty;
    public DateOnly ExamDate { get; set; }
    public int? Grade { get; set; }
    public string Status { get; set; } = string.Empty;
}
