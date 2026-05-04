using System.ComponentModel.DataAnnotations;

namespace ExamManagement.Api.DTOs;

public sealed class LessonCreateDto
{
    [Required(ErrorMessage = "Lesson code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Lesson code must be exactly 3 characters.")]
    [RegularExpression("^[a-zA-Z0-9]{3}$", ErrorMessage = "Lesson code must contain only letters and digits.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson name is required.")]
    [MaxLength(30, ErrorMessage = "Lesson name cannot exceed 30 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Class level must be between 1 and 12.")]
    public int ClassLevel { get; set; }
}

public sealed class LessonUpdateDto
{
    [Required(ErrorMessage = "Lesson code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Lesson code must be exactly 3 characters.")]
    [RegularExpression("^[a-zA-Z0-9]{3}$", ErrorMessage = "Lesson code must contain only letters and digits.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson name is required.")]
    [MaxLength(30, ErrorMessage = "Lesson name cannot exceed 30 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Class level must be between 1 and 12.")]
    public int ClassLevel { get; set; }
}

public sealed class LessonDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ClassLevel { get; set; }
    public string TeacherFirstName { get; set; } = string.Empty;
    public string TeacherLastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
