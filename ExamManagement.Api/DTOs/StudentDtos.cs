using System.ComponentModel.DataAnnotations;

namespace ExamManagement.Api.DTOs;

public sealed class StudentCreateDto
{
    [Range(1, 99999, ErrorMessage = "Student number must be between 1 and 99999.")]
    public int Number { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(30, ErrorMessage = "First name cannot exceed 30 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Class level must be between 1 and 12.")]
    public int ClassLevel { get; set; }
}

public sealed class StudentUpdateDto
{
    [Range(1, 99999, ErrorMessage = "Student number must be between 1 and 99999.")]
    public int Number { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(30, ErrorMessage = "First name cannot exceed 30 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Class level must be between 1 and 12.")]
    public int ClassLevel { get; set; }
}

public sealed class StudentDto
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int ClassLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
