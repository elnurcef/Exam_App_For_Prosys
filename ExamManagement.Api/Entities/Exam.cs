namespace ExamManagement.Api.Entities;

public sealed class Exam
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int StudentId { get; set; }
    public DateOnly ExamDate { get; set; }
    public byte? Grade { get; set; }
    public string Status { get; set; } = ExamStatus.Scheduled;
    public int TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
