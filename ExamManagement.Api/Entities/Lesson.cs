namespace ExamManagement.Api.Entities;

public sealed class Lesson
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public byte ClassLevel { get; set; }
    public string TeacherFirstName { get; set; } = string.Empty;
    public string TeacherLastName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public ICollection<Exam> Exams { get; set; } = [];
}
