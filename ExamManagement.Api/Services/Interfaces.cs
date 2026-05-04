using ExamManagement.Api.DTOs;
using ExamManagement.Api.Entities;

namespace ExamManagement.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Teacher teacher);
}

public interface IAuthService
{
    Task<AuthResponseDto> SignupAsync(SignupRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> GetMeAsync(int teacherId, CancellationToken cancellationToken);
}

public interface ILessonService
{
    Task<IReadOnlyList<LessonDto>> GetAllAsync(int teacherId, CancellationToken cancellationToken);
    Task<LessonDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken);
    Task<LessonDto> CreateAsync(int teacherId, LessonCreateDto request, CancellationToken cancellationToken);
    Task<LessonDto> UpdateAsync(int teacherId, int id, LessonUpdateDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken);
}

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(int teacherId, CancellationToken cancellationToken);
    Task<StudentDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken);
    Task<StudentDto> CreateAsync(int teacherId, StudentCreateDto request, CancellationToken cancellationToken);
    Task<StudentDto> UpdateAsync(int teacherId, int id, StudentUpdateDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken);
}

public interface IExamService
{
    Task<IReadOnlyList<ExamDto>> GetAllAsync(int teacherId, ExamFilterDto filter, CancellationToken cancellationToken);
    Task<ExamDto> GetByIdAsync(int teacherId, int id, CancellationToken cancellationToken);
    Task<ExamDto> ScheduleAsync(int teacherId, ExamScheduleDto request, CancellationToken cancellationToken);
    Task<ExamDto> UpdateAsync(int teacherId, int id, ExamUpdateDto request, CancellationToken cancellationToken);
    Task<ExamDto> GradeAsync(int teacherId, int id, ExamGradeDto request, CancellationToken cancellationToken);
    Task<ExamDto> CancelAsync(int teacherId, int id, CancellationToken cancellationToken);
    Task DeleteAsync(int teacherId, int id, CancellationToken cancellationToken);
}

public interface IReportService
{
    Task<IReadOnlyList<ExamResultDto>> GetExamResultsAsync(int teacherId, ExamFilterDto filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamResultDto>> GetStudentReportAsync(int teacherId, int studentId, ExamFilterDto filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamResultDto>> GetLessonReportAsync(int teacherId, int lessonId, ExamFilterDto filter, CancellationToken cancellationToken);
}
