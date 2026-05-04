using ExamManagement.Api.DTOs;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> StudentReport(
        int studentId,
        [FromQuery] ExamFilterDto filter,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetStudentReportAsync(User.GetTeacherId(), studentId, filter, cancellationToken));
    }

    [HttpGet("lesson/{lessonId:int}")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> LessonReport(
        int lessonId,
        [FromQuery] ExamFilterDto filter,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetLessonReportAsync(User.GetTeacherId(), lessonId, filter, cancellationToken));
    }

    [HttpGet("exam-results")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> ExamResults(
        [FromQuery] ExamFilterDto filter,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetExamResultsAsync(User.GetTeacherId(), filter, cancellationToken));
    }
}
