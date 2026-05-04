namespace ExamManagement.Api.Entities;

public sealed class Student
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public byte ClassLevel { get; set; }
    public int TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public ICollection<Exam> Exams { get; set; } = [];
}
