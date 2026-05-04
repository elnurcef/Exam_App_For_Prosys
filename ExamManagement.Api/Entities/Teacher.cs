namespace ExamManagement.Api.Entities;

public sealed class Teacher
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<Exam> Exams { get; set; } = [];
}
